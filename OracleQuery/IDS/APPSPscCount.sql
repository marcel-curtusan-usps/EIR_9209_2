-- APPS INDUCTED = Total - No Induction# + Manual Keyed
--          = Count(*) - (INDUCTNUMBER = 0) + (VCSDESKID = 1)
-- EPPS INDUCTED = Total - No Induction#
--          = Count(*) - INDUCTNUMBER = 0
-- 11/27/2024 Reject by Inventory_Map REJ, Rework, Manual label bin
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
                SELECT /*+ PARALLEL(A, 32)*/
                    L.MPE_NAME,
                    (
                        CASE
                            WHEN L.MPE_TYPE='EPPS' AND A.INDUCTNUMBER = 0 THEN
                                'PreScanned'
                            WHEN L.MPE_TYPE='APPS' AND A.INDUCTNUMBER = 0 THEN
                                'PreScanned'
                            WHEN UPPER(I.DEST_DISPLAY_TEXT) LIKE '%REJ%' OR UPPER(I.DEST_DISPLAY_TEXT) LIKE '%REWORK%' OR UPPER(I.DEST_DISPLAY_TEXT) LIKE '%MANUAL%' OR I.DEST_DISPLAY_TEXT IS NULL THEN
                                'Rejected'
                            ELSE
                                'Sorted'
                        END)AS                                RESULT,
                    TO_CHAR(A.DATA_DATE, 'yyyy-mm-dd hh24') AS HR,
                    COUNT(*)                                AS COUNT
                FROM
                    DCSDBA.APPSMP        A,
                    DCSDBA.MPE_LIST      L,
                    DCSDBA.INVENTORY_MAP I
                WHERE
                    A.DATA_DAY in (:DATADAYLIST)
                    AND A.MPE_ID IN (
                        SELECT
                            MPE_ID
                        FROM
                            DCSDBA.MPE_LIST
                        WHERE
                            MPE_TYPE IN ('APPS', 'EPPS')
                    )
                    AND A.MPE_ID = L.MPE_ID
                    AND A.SORTPLAN_ID = I.SORTPLAN_ID
                    AND A.POCKETNUM = I.POCKET_NUM
                GROUP BY
                    L.MPE_NAME,
                    (
                        CASE
                            WHEN L.MPE_TYPE='EPPS' AND A.INDUCTNUMBER = 0 THEN
                                'PreScanned'
                            WHEN L.MPE_TYPE='APPS' AND A.INDUCTNUMBER = 0 THEN
                                'PreScanned'
                            WHEN UPPER(I.DEST_DISPLAY_TEXT) LIKE '%REJ%' OR UPPER(I.DEST_DISPLAY_TEXT) LIKE '%REWORK%' OR UPPER(I.DEST_DISPLAY_TEXT) LIKE '%MANUAL%' OR I.DEST_DISPLAY_TEXT IS NULL THEN
                                'Rejected'
                            ELSE
                                'Sorted'
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
