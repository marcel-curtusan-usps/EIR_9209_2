-- APPS INDUCTED = Total - No Induction# + Manual Keyed
--          = Count(*) - (INDUCTNUMBER = 0) + (VCSDESKID = 1)
-- EPPS INDUCTED = Total - No Induction#
--          = Count(*) - INDUCTNUMBER = 0
-- 11/27/2024 Reject by Inventory_Map REJ, Rework, Manual label bin
with mpe_apps as (
   select mpe_id, mpe_name, mpe_type
     from dcsdba.mpe_list
     where regexp_like (mpe_type,'^(APPS|EPPS|SPBSTS|APBS)')
), apps as (
   select /*+ PARALLEL(a, 32) */ l.mpe_name,
          to_char(a.data_date, 'yyyy-mm-dd hh24') as hr,
          case
             when a.inductnumber = 0 then 'PreScanned'
             when upper(i.dest_display_text) like '%REJ%'
               or upper(i.dest_display_text) like '%REWORK%'
               or upper(i.dest_display_text) like '%MANUAL%'
               or i.dest_display_text is null then 'Rejected'
             else 'Sorted'
          end as result_label
     from dcsdba.appsmp a
     join mpe_apps l
       on a.mpe_id = l.mpe_id
     join dcsdba.inventory_map i
       on a.sortplan_id = i.sortplan_id
      and a.pocketnum = i.pocket_num
    where instr(','||:DATADAYLIST||',', ','||trim(a.data_day)||',') > 0
), apbs as (
   select /*+ PARALLEL(a, 32) */ l.mpe_name,
          to_char(a.data_date, 'yyyy-mm-dd hh24') as hr,
          case
           when instr(','||:REWORKBINS||',', ','||trim(a.pocketnum)||',') > 0 then 'Reworked'
           when  instr(','||:REJECTBINS||',', ','||trim(a.pocketnum)||',') > 0 then 'Rejected'
           else 'Sorted'
          end as result_label
     from dcsdba.appsmp a
     join mpe_apps l
       on a.mpe_id = l.mpe_id
    where instr(','||:DATADAYLIST||',', ','||trim(a.data_day)||',') > 0
), epps as (
   select /*+ PARALLEL(a, 32) */ l.mpe_name,
          to_char(a.data_date, 'yyyy-mm-dd hh24') as hr,
          case
             when a.inductnumber = 0 then 'PreScanned'
             when upper(i.dest_display_text) like '%REJ%'
               or upper(i.dest_display_text) like '%REWORK%'
               or upper(i.dest_display_text) like '%MANUAL%'
               or i.dest_display_text is null then 'Rejected'
             else 'Sorted'
          end as result_label
     from dcsdba.appsmp a
     join mpe_apps l
       on a.mpe_id = l.mpe_id
     join dcsdba.inventory_map i
       on a.sortplan_id = i.sortplan_id
      and a.pocketnum = i.pocket_num
    where instr(','||:DATADAYLIST||',', ','||trim(a.data_day)||',') > 0
), all_events as (
   select * from apps
   union all
   select * from epps
   union all
   select * from apbs

)
select mpe_name,
       hr || ':00' as hour,
       nvl(sum(case when result_label = 'Sorted' then 1 else 0 end),0) as sorted,
       nvl(sum(case when result_label = 'Rejected' then 1 else 0 end),0) as rejected,
       nvl(sum(case when result_label in ('Sorted','Rejected','PreScanned','Reworked') then 1 else 0 end),0) as inducted
  from all_events
 group by mpe_name, hr
 order by mpe_name, hr
