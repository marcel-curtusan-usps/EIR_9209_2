with filtered as (
  /* narrow to only candidate rows and extract ip once */
   select /*+ PARALLEL(A, 16)*/
    mpe_id,
          change_date,
          regexp_substr(
             data,
             '([0-9]{1,3}(\.[0-9]{1,3}){3})',
             1,
             1,
             null,
             1
          ) as ip_address,
          -- extract mpe_name: text between "session" and "connected"/"end"/"ended"
          trim(regexp_substr(
             data,
             'session[[:space:]]+(.+)[[:space:]]+(connected|end|ended)',
             1,
             1,
             'i',
             1
          )) as mpe_name,
          case
             when instr(
                   upper(data),
                   'CONNECTED'
                ) > 0
                and regexp_like ( data,
                                  '^(SBIOS|IOS) session .* connected',
                                  'i' ) then
                'CON'
             when instr(
                   upper(data),
                   'END'
                ) > 0
                and regexp_like ( data,
                                  '^(SBIOS|IOS) session .* (end|ended)',
                                  'i' ) then
                'END'
             else
                null
          end as evt,
          data
     from machine_status
    where instr(
         upper(data),
         'IOS SESSION'
      ) > 0
      and change_date between sysdate - 2 and sysdate
      order by mpe_name
    -- Optional: add date filter to speed up large tables:
      --
),latest_connect as (
  /* one most-recent CONNECT per mpe_id */
   select mpe_id,
          ip_address,
          mpe_name,
          change_date as conn_dt,
          data as conn_log
     from (
      select f.*,
             row_number()
             over(partition by mpe_id
                  order by change_date desc
             ) rn
        from filtered f
       where evt = 'CON'
   )
    where rn = 1
),end_info as (
  /* for each chosen connect, find the earliest END after it (NULL if none) */
   select lc.*,
          (
             select min(f2.change_date)
               from filtered f2
              where f2.evt = 'END'
                and f2.mpe_id = lc.mpe_id
                and f2.ip_address = lc.ip_address
                and f2.change_date > lc.conn_dt
          ) as end_dt
     from latest_connect lc
)
select ei.mpe_id,
       ei.mpe_name,
       ei.ip_address as connect_ip,
       to_char(
          ei.conn_dt,
          'YYYY-MM-DD"T"HH24:MI:SS'
       ) as connected_dt,
       to_char(
          ei.end_dt,
          'YYYY-MM-DD"T"HH24:MI:SS'
       ) as disconnected_dt,
       case
          when ei.end_dt is not null then
             round((ei.end_dt - ei.conn_dt) * 24 * 3600)
       end as duration_seconds,
       case
          when ei.end_dt is not null then
             numtodsinterval(
                ei.end_dt - ei.conn_dt,
                'DAY'
             )
       end as duration_interval
  from end_info ei
  left join filtered fe
on fe.mpe_id = ei.mpe_id
   and fe.ip_address = ei.ip_address
   and fe.change_date = ei.end_dt
 order by ei.mpe_name;
