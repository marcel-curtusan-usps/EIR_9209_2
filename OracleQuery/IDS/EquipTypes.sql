WITH all_mpe_types AS (
    SELECT /*+ PARALLEL(8) */ 'AFCSMP' AS table_name, m.mpe_type
      FROM AFCSMP t
      JOIN MPE_LIST m ON t.mpe_id = m.mpe_id
    UNION ALL
    SELECT /*+ PARALLEL(8) */ 'APPSMP', m.mpe_type
      FROM APPSMP t
      JOIN MPE_LIST m ON t.mpe_id = m.mpe_id
    UNION ALL
    SELECT /*+ PARALLEL(8) */ 'BCSMP', m.mpe_type
      FROM BCSMP t
      JOIN MPE_LIST m ON t.mpe_id = m.mpe_id
    UNION ALL
    SELECT /*+ PARALLEL(8) */ 'FMPCSMP', m.mpe_type
      FROM FMPCSMP t
      JOIN MPE_LIST m ON t.mpe_id = m.mpe_id
    UNION ALL
    SELECT /*+ PARALLEL(8) */ 'FSMMP', m.mpe_type
      FROM FSMMP t
      JOIN MPE_LIST m ON t.mpe_id = m.mpe_id
    UNION ALL
    SELECT /*+ PARALLEL(8) */ 'GENMP', m.mpe_type
      FROM GENMP t
      JOIN MPE_LIST m ON t.mpe_id = m.mpe_id
)
SELECT /*+ PARALLEL(8) */
       table_name,
       LISTAGG(DISTINCT mpe_type, ',') WITHIN GROUP (ORDER BY mpe_type) AS mpe_types
  FROM all_mpe_types
GROUP BY table_name
ORDER BY table_name;