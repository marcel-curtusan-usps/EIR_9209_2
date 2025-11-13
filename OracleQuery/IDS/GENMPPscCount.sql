with mpe_list as (
   select mpe_id, mpe_name
     from dcsdba.mpe_list
    where regexp_like (mpe_type,'^(HOPS|SPBSTS|APBS)')
), hops as (
   select /*+ PARALLEL(u, 16) */ l.mpe_name,
          to_char(u.data_date, 'yyyy-mm-dd hh24') as hr,
          -- produce a normalized result label similar to FMPCSPscCount.sql
          case
             when u.pocketnum = 401 or u.pocketnum = 404 then 'Reworked'
             when u.pocketnum >= 198 or u.pocketnum <= 2 or u.zip = 'NOREAD' then 'Rejected'
             else 'Sorted'
          end as result_label
     from dcsdba.genmp u
     join mpe_list l
       on u.mpe_id = l.mpe_id
    and instr(','||:DATADAYLIST||',', ','||trim(u.data_day)||',') > 0
), apbs as(
    select /*+ PARALLEL(u, 16) */ l.mpe_name,
          to_char(a.data_date, 'yyyy-mm-dd hh24') as hr,
          -- produce a normalized result label similar to FMPCSPscCount.sql
          case
            when a.pocketnum = 2 then 'Reworked'
            when a.pocketnum = 205 then 'IndFail'
            when a.pocketnum = 1 then 'Rejected'
            else 'Sorted'
          end as result_label
     from dcsdba.genmp a
     join mpe_list l
       on a.mpe_id = l.mpe_id
    and instr(','||:DATADAYLIST||',', ','||trim(a.data_day)||',') > 0
),
all_events as (
   select * from apbs
)
select mpe_name,
       hr || ':00' as hour,
       nvl(sum(case when result_label = 'Sorted' then 1 else 0 end), 0) as sorted,
       nvl(sum(case when result_label = 'Rejected' then 1 else 0 end), 0) as rejected,
       nvl(sum(case when result_label in ('Sorted','Rejected','IndFail') then 1 else 0 end), 0) as inducted
  from all_events
 group by mpe_name, hr
 order by mpe_name, hr
