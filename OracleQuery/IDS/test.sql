with mpe_hsus as (
   select mpe_id,
          mpe_name
     from dcsdba.mpe_list
    where regexp_like ( mpe_type,
                        '^(HSUS)' )
),fm as (
   select /*+ PARALLEL(f, 8) */ l.mpe_name,
          to_char(
             f.data_date,
             'yyyy-mm-dd hh24'
          ) as hr,
          case f.result
             when 'Normal'    then
                'Sorted'
             when 'Processed' then
                'Sorted'
             when 'Missent'   then
                'Rejected'
             else
                f.result
          end as result_label
     from dcsdba.fmpcsmp f
     join mpe_hsus l
   on f.mpe_id = l.mpe_id
      and regexp_like ( trim(f.data_day), '^[0-9]+$' )
      and instr(
   ','
   || :DATADAYLIST
   || ',',
   ','
   || trim(f.data_day)
   || ','
) > 0
),all_events as (
   select *
     from fm
) select mpe_name,
       hr || ':00' as hour,
       nvl(
          sum(
             case
                when result_label = 'Sorted' then
                   1
                else
                   0
             end
          ),
          0
       ) as sorted,
       nvl(
          sum(
             case
                when result_label = 'Rejected' then
                   1
                else
                   0
             end
          ),
          0
       ) as rejected,
       nvl(
          sum(
             case
                when result_label in('Sorted',
                                     'Rejected',
                                     'IndFail') then
                   1
                else
                   0
             end
          ),
          0
       ) as inducted
  from all_events
 group by mpe_name,
          hr
 order by mpe_name