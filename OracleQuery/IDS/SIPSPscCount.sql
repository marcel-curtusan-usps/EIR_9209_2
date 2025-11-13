-- SIPS mpe_type='SDUS'
--     and MPE_SERIAL_NUM like '%PSS%'
-- SIPS REJECTED POCKETNUM=201
SELECT
    MPE_NAME,
    HR
    ||':00'                         AS HOUR,
    NVL(SORTED, 0)                  AS SORTED,
    NVL(REJECTED, 0)                AS REJECTED,
    NVL(SORTED, 0)+NVL(REJECTED, 0) AS INDUCTED
FROM
    (
        SELECT
            S.MPE_NAME,
            S.HR,
            S.RESULT,
            SUM(S.COUNT) AS "TOTAL"
        FROM
            (
                SELECT /*+ PARALLEL(A, 16)*/
                    L.MPE_NAME                              AS MPE_NAME,
                    (
                        CASE
                            WHEN A.POCKETNUM NOT IN (:REJECTBINS) THEN
                                'Sorted'
                            ELSE
                                'Rejected'
                        END)                                AS RESULT,
                    TO_CHAR(A.DATA_DATE, 'yyyy-mm-dd hh24') AS HR,
                    COUNT(*)                                AS COUNT
                FROM
                    DCSDBA.APPSMP   A,
                    DCSDBA.MPE_LIST L
                WHERE
                    A.MPE_ID IN (
                        SELECT
                            MPE_ID
                        FROM
                            DCSDBA.MPE_LIST
                        WHERE
                            MPE_TYPE IN ('SIPS')
                    )
                    -- Accept a comma-separated bind string in :DATADAYLIST (e.g. '20231101,20231102').
                    -- Use INSTR to match CSV values to avoid implicit numeric conversion errors (ORA-01722).
                    AND instr(','||:DATADAYLIST||',', ','||trim(A.DATA_DAY)||',') > 0
                    AND A.MPE_ID = L.MPE_ID
                GROUP BY
                    L.MPE_NAME,
                    (
                        CASE
                            WHEN A.POCKETNUM NOT IN (:REJECTBINS) THEN
                                'Sorted'
                            ELSE
                                'Rejected'
                        END),
                    TO_CHAR(A.DATA_DATE, 'yyyy-mm-dd hh24')
                ORDER BY
                    HR
            )S
        GROUP BY
            MPE_NAME,
            HR,
            RESULT
        ORDER BY
            MPE_NAME,
            HR,
            RESULT
    ) PIVOT ( MIN(TOTAL) FOR (RESULT) IN ('Sorted' AS SORTED,
    'Rejected' AS REJECTED,
    'Inducted' AS INDUCTED,
    'PreScanned' AS PRESCANNED) )
ORDER BY
    MPE_NAME,
    HOUR
