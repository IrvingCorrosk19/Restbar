-- Cleanup duplicate Armado stations and prep steps for S71
DELETE FROM product_preparation_steps
WHERE id NOT IN (
    SELECT MIN(id::text)::uuid
    FROM product_preparation_steps
    GROUP BY product_id, step_order
);

UPDATE product_preparation_steps pps
SET station_id = s.id
FROM stations s
WHERE s.name = 'Armado' AND s.branch_id = pps.branch_id
  AND pps.step_order = 2
  AND pps.station_id IN (
    SELECT id FROM stations WHERE name = 'Armado'
    EXCEPT
    SELECT MIN(id) FROM stations WHERE name = 'Armado' GROUP BY branch_id
  );

DELETE FROM stations s
WHERE s.name = 'Armado'
  AND s.id NOT IN (
    SELECT MIN(id) FROM stations WHERE name = 'Armado' AND branch_id = s.branch_id GROUP BY branch_id
  );
