-- MEDEVAX Medical Solutions catalogue import
-- Source: MEDEVAX_Medical_Solutions_Catalogue.xlsx
-- Imports the 167 numbered products present in the source sheet and links their local PNG images.
-- The workbook cover says 174 products; the actual numbered list contains 167.
-- No drug products are invented: pharmaceutical categories are created and left ready for approved future product data.
BEGIN;

-- Existing test/developer data is intentionally left untouched by this import.

INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Pharmaceuticals & Medicines', NULL, NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories"
    WHERE "CategoryName" = 'Pharmaceuticals & Medicines' AND "ParentCategoryId" IS NULL
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Supplements & Wellness', NULL, NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories"
    WHERE "CategoryName" = 'Supplements & Wellness' AND "ParentCategoryId" IS NULL
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Hospital & Medical Equipment', NULL, NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories"
    WHERE "CategoryName" = 'Hospital & Medical Equipment' AND "ParentCategoryId" IS NULL
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Laboratory Equipment', NULL, NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories"
    WHERE "CategoryName" = 'Laboratory Equipment' AND "ParentCategoryId" IS NULL
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Laboratory Reagents', NULL, NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories"
    WHERE "CategoryName" = 'Laboratory Reagents' AND "ParentCategoryId" IS NULL
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Laboratory Consumables', NULL, NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories"
    WHERE "CategoryName" = 'Laboratory Consumables' AND "ParentCategoryId" IS NULL
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Diagnostic Equipment', NULL, NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories"
    WHERE "CategoryName" = 'Diagnostic Equipment' AND "ParentCategoryId" IS NULL
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Rapid Diagnostic & Test Kits', NULL, NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories"
    WHERE "CategoryName" = 'Rapid Diagnostic & Test Kits' AND "ParentCategoryId" IS NULL
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Medical Sundries', NULL, NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories"
    WHERE "CategoryName" = 'Medical Sundries' AND "ParentCategoryId" IS NULL
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Surgical', NULL, NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories"
    WHERE "CategoryName" = 'Surgical' AND "ParentCategoryId" IS NULL
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Theatre / Operating Room', NULL, NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories"
    WHERE "CategoryName" = 'Theatre / Operating Room' AND "ParentCategoryId" IS NULL
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Patient Care', NULL, NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories"
    WHERE "CategoryName" = 'Patient Care' AND "ParentCategoryId" IS NULL
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Wound Care', NULL, NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories"
    WHERE "CategoryName" = 'Wound Care' AND "ParentCategoryId" IS NULL
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Disposable Medical Supplies', NULL, NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories"
    WHERE "CategoryName" = 'Disposable Medical Supplies' AND "ParentCategoryId" IS NULL
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'PPE & Infection Prevention', NULL, NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories"
    WHERE "CategoryName" = 'PPE & Infection Prevention' AND "ParentCategoryId" IS NULL
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Waste Management', NULL, NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories"
    WHERE "CategoryName" = 'Waste Management' AND "ParentCategoryId" IS NULL
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Medical Furniture', NULL, NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories"
    WHERE "CategoryName" = 'Medical Furniture' AND "ParentCategoryId" IS NULL
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Emergency & First Aid', NULL, NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories"
    WHERE "CategoryName" = 'Emergency & First Aid' AND "ParentCategoryId" IS NULL
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Sterilization & Disinfection', NULL, NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories"
    WHERE "CategoryName" = 'Sterilization & Disinfection' AND "ParentCategoryId" IS NULL
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Physiotherapy & Rehabilitation', NULL, NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories"
    WHERE "CategoryName" = 'Physiotherapy & Rehabilitation' AND "ParentCategoryId" IS NULL
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Homecare', NULL, NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories"
    WHERE "CategoryName" = 'Homecare' AND "ParentCategoryId" IS NULL
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Respiratory Care', NULL, NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories"
    WHERE "CategoryName" = 'Respiratory Care' AND "ParentCategoryId" IS NULL
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Cold Chain & Vaccine Equipment', NULL, NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories"
    WHERE "CategoryName" = 'Cold Chain & Vaccine Equipment' AND "ParentCategoryId" IS NULL
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Cardiology', NULL, NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories"
    WHERE "CategoryName" = 'Cardiology' AND "ParentCategoryId" IS NULL
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Neurology', NULL, NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories"
    WHERE "CategoryName" = 'Neurology' AND "ParentCategoryId" IS NULL
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Specimen Collection', NULL, NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories"
    WHERE "CategoryName" = 'Specimen Collection' AND "ParentCategoryId" IS NULL
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Hospital Trolleys & Storage', NULL, NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories"
    WHERE "CategoryName" = 'Hospital Trolleys & Storage' AND "ParentCategoryId" IS NULL
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'General Hospital Supplies', NULL, NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories"
    WHERE "CategoryName" = 'General Hospital Supplies' AND "ParentCategoryId" IS NULL
);

INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Class A Drugs',
       (SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Pharmaceuticals & Medicines' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1),
       NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c
    WHERE c."CategoryName" = 'Class A Drugs'
      AND c."ParentCategoryId" = (
        SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Pharmaceuticals & Medicines' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1
      )
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Class B Drugs',
       (SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Pharmaceuticals & Medicines' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1),
       NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c
    WHERE c."CategoryName" = 'Class B Drugs'
      AND c."ParentCategoryId" = (
        SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Pharmaceuticals & Medicines' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1
      )
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Other Pharmaceutical Products',
       (SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Pharmaceuticals & Medicines' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1),
       NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c
    WHERE c."CategoryName" = 'Other Pharmaceutical Products'
      AND c."ParentCategoryId" = (
        SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Pharmaceuticals & Medicines' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1
      )
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Vitamins & Nutritional Supplements',
       (SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Supplements & Wellness' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1),
       NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c
    WHERE c."CategoryName" = 'Vitamins & Nutritional Supplements'
      AND c."ParentCategoryId" = (
        SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Supplements & Wellness' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1
      )
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Wellness Products',
       (SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Supplements & Wellness' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1),
       NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c
    WHERE c."CategoryName" = 'Wellness Products'
      AND c."ParentCategoryId" = (
        SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Supplements & Wellness' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1
      )
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'General Hospital Equipment',
       (SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Hospital & Medical Equipment' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1),
       NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c
    WHERE c."CategoryName" = 'General Hospital Equipment'
      AND c."ParentCategoryId" = (
        SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Hospital & Medical Equipment' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1
      )
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Patient Monitoring',
       (SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Hospital & Medical Equipment' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1),
       NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c
    WHERE c."CategoryName" = 'Patient Monitoring'
      AND c."ParentCategoryId" = (
        SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Hospital & Medical Equipment' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1
      )
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Infusion & Medication Delivery',
       (SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Hospital & Medical Equipment' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1),
       NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c
    WHERE c."CategoryName" = 'Infusion & Medication Delivery'
      AND c."ParentCategoryId" = (
        SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Hospital & Medical Equipment' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1
      )
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Emergency & Resuscitation',
       (SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Hospital & Medical Equipment' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1),
       NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c
    WHERE c."CategoryName" = 'Emergency & Resuscitation'
      AND c."ParentCategoryId" = (
        SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Hospital & Medical Equipment' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1
      )
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Respiratory Equipment',
       (SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Hospital & Medical Equipment' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1),
       NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c
    WHERE c."CategoryName" = 'Respiratory Equipment'
      AND c."ParentCategoryId" = (
        SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Hospital & Medical Equipment' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1
      )
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Microscopes & Optical Equipment',
       (SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Laboratory Equipment' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1),
       NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c
    WHERE c."CategoryName" = 'Microscopes & Optical Equipment'
      AND c."ParentCategoryId" = (
        SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Laboratory Equipment' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1
      )
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Laboratory Processing Equipment',
       (SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Laboratory Equipment' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1),
       NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c
    WHERE c."CategoryName" = 'Laboratory Processing Equipment'
      AND c."ParentCategoryId" = (
        SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Laboratory Equipment' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1
      )
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Laboratory Heating & Incubation',
       (SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Laboratory Equipment' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1),
       NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c
    WHERE c."CategoryName" = 'Laboratory Heating & Incubation'
      AND c."ParentCategoryId" = (
        SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Laboratory Equipment' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1
      )
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Laboratory Mixing & Shaking',
       (SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Laboratory Equipment' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1),
       NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c
    WHERE c."CategoryName" = 'Laboratory Mixing & Shaking'
      AND c."ParentCategoryId" = (
        SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Laboratory Equipment' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1
      )
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Laboratory Pipetting',
       (SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Laboratory Equipment' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1),
       NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c
    WHERE c."CategoryName" = 'Laboratory Pipetting'
      AND c."ParentCategoryId" = (
        SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Laboratory Equipment' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1
      )
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Biosafety & Containment',
       (SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Laboratory Equipment' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1),
       NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c
    WHERE c."CategoryName" = 'Biosafety & Containment'
      AND c."ParentCategoryId" = (
        SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Laboratory Equipment' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1
      )
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Basic Laboratory Equipment',
       (SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Laboratory Equipment' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1),
       NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c
    WHERE c."CategoryName" = 'Basic Laboratory Equipment'
      AND c."ParentCategoryId" = (
        SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Laboratory Equipment' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1
      )
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Microbiology Culture Media',
       (SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Laboratory Reagents' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1),
       NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c
    WHERE c."CategoryName" = 'Microbiology Culture Media'
      AND c."ParentCategoryId" = (
        SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Laboratory Reagents' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1
      )
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Serology & Immunology Reagents',
       (SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Laboratory Reagents' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1),
       NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c
    WHERE c."CategoryName" = 'Serology & Immunology Reagents'
      AND c."ParentCategoryId" = (
        SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Laboratory Reagents' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1
      )
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Microscopy & Staining Reagents',
       (SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Laboratory Reagents' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1),
       NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c
    WHERE c."CategoryName" = 'Microscopy & Staining Reagents'
      AND c."ParentCategoryId" = (
        SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Laboratory Reagents' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1
      )
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Sample Handling',
       (SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Laboratory Consumables' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1),
       NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c
    WHERE c."CategoryName" = 'Sample Handling'
      AND c."ParentCategoryId" = (
        SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Laboratory Consumables' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1
      )
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'General Laboratory Consumables',
       (SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Laboratory Consumables' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1),
       NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c
    WHERE c."CategoryName" = 'General Laboratory Consumables'
      AND c."ParentCategoryId" = (
        SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Laboratory Consumables' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1
      )
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Basic Diagnostic Equipment',
       (SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Diagnostic Equipment' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1),
       NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c
    WHERE c."CategoryName" = 'Basic Diagnostic Equipment'
      AND c."ParentCategoryId" = (
        SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Diagnostic Equipment' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1
      )
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Diabetes Monitoring',
       (SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Diagnostic Equipment' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1),
       NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c
    WHERE c."CategoryName" = 'Diabetes Monitoring'
      AND c."ParentCategoryId" = (
        SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Diagnostic Equipment' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1
      )
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Hematology Analysers',
       (SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Diagnostic Equipment' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1),
       NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c
    WHERE c."CategoryName" = 'Hematology Analysers'
      AND c."ParentCategoryId" = (
        SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Diagnostic Equipment' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1
      )
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'General Rapid Tests',
       (SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Rapid Diagnostic & Test Kits' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1),
       NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c
    WHERE c."CategoryName" = 'General Rapid Tests'
      AND c."ParentCategoryId" = (
        SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Rapid Diagnostic & Test Kits' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1
      )
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Malaria Tests',
       (SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Rapid Diagnostic & Test Kits' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1),
       NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c
    WHERE c."CategoryName" = 'Malaria Tests'
      AND c."ParentCategoryId" = (
        SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Rapid Diagnostic & Test Kits' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1
      )
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'General Medical Sundries',
       (SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Medical Sundries' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1),
       NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c
    WHERE c."CategoryName" = 'General Medical Sundries'
      AND c."ParentCategoryId" = (
        SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Medical Sundries' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1
      )
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Diagnostic Consumables',
       (SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Medical Sundries' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1),
       NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c
    WHERE c."CategoryName" = 'Diagnostic Consumables'
      AND c."ParentCategoryId" = (
        SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Medical Sundries' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1
      )
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Surgical Instruments',
       (SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Surgical' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1),
       NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c
    WHERE c."CategoryName" = 'Surgical Instruments'
      AND c."ParentCategoryId" = (
        SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Surgical' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1
      )
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Surgical Consumables',
       (SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Surgical' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1),
       NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c
    WHERE c."CategoryName" = 'Surgical Consumables'
      AND c."ParentCategoryId" = (
        SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Surgical' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1
      )
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Theatre Supplies',
       (SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Theatre / Operating Room' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1),
       NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c
    WHERE c."CategoryName" = 'Theatre Supplies'
      AND c."ParentCategoryId" = (
        SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Theatre / Operating Room' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1
      )
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Examination & Vital Signs',
       (SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Patient Care' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1),
       NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c
    WHERE c."CategoryName" = 'Examination & Vital Signs'
      AND c."ParentCategoryId" = (
        SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Patient Care' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1
      )
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Mobility & Patient Support',
       (SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Patient Care' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1),
       NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c
    WHERE c."CategoryName" = 'Mobility & Patient Support'
      AND c."ParentCategoryId" = (
        SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Patient Care' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1
      )
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Plasters & Dressings',
       (SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Wound Care' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1),
       NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c
    WHERE c."CategoryName" = 'Plasters & Dressings'
      AND c."ParentCategoryId" = (
        SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Wound Care' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1
      )
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Syringes & Injection Supplies',
       (SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Disposable Medical Supplies' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1),
       NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c
    WHERE c."CategoryName" = 'Syringes & Injection Supplies'
      AND c."ParentCategoryId" = (
        SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Disposable Medical Supplies' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1
      )
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Disposable Procedure Supplies',
       (SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Disposable Medical Supplies' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1),
       NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c
    WHERE c."CategoryName" = 'Disposable Procedure Supplies'
      AND c."ParentCategoryId" = (
        SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Disposable Medical Supplies' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1
      )
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Gloves',
       (SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'PPE & Infection Prevention' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1),
       NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c
    WHERE c."CategoryName" = 'Gloves'
      AND c."ParentCategoryId" = (
        SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'PPE & Infection Prevention' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1
      )
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Masks & Respiratory Protection',
       (SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'PPE & Infection Prevention' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1),
       NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c
    WHERE c."CategoryName" = 'Masks & Respiratory Protection'
      AND c."ParentCategoryId" = (
        SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'PPE & Infection Prevention' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1
      )
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Protective Clothing & Eye Protection',
       (SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'PPE & Infection Prevention' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1),
       NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c
    WHERE c."CategoryName" = 'Protective Clothing & Eye Protection'
      AND c."ParentCategoryId" = (
        SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'PPE & Infection Prevention' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1
      )
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Waste Bins & Liners',
       (SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Waste Management' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1),
       NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c
    WHERE c."CategoryName" = 'Waste Bins & Liners'
      AND c."ParentCategoryId" = (
        SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Waste Management' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1
      )
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Sharps Disposal',
       (SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Waste Management' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1),
       NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c
    WHERE c."CategoryName" = 'Sharps Disposal'
      AND c."ParentCategoryId" = (
        SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Waste Management' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1
      )
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Laboratory Furniture',
       (SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Medical Furniture' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1),
       NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c
    WHERE c."CategoryName" = 'Laboratory Furniture'
      AND c."ParentCategoryId" = (
        SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Medical Furniture' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1
      )
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'General Medical Furniture',
       (SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Medical Furniture' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1),
       NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c
    WHERE c."CategoryName" = 'General Medical Furniture'
      AND c."ParentCategoryId" = (
        SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Medical Furniture' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1
      )
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'First Aid Kits',
       (SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Emergency & First Aid' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1),
       NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c
    WHERE c."CategoryName" = 'First Aid Kits'
      AND c."ParentCategoryId" = (
        SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Emergency & First Aid' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1
      )
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Emergency Supplies',
       (SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Emergency & First Aid' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1),
       NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c
    WHERE c."CategoryName" = 'Emergency Supplies'
      AND c."ParentCategoryId" = (
        SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Emergency & First Aid' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1
      )
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Sterilizers & Autoclaves',
       (SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Sterilization & Disinfection' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1),
       NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c
    WHERE c."CategoryName" = 'Sterilizers & Autoclaves'
      AND c."ParentCategoryId" = (
        SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Sterilization & Disinfection' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1
      )
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Disinfectants',
       (SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Sterilization & Disinfection' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1),
       NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c
    WHERE c."CategoryName" = 'Disinfectants'
      AND c."ParentCategoryId" = (
        SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Sterilization & Disinfection' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1
      )
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Rehabilitation Equipment',
       (SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Physiotherapy & Rehabilitation' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1),
       NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c
    WHERE c."CategoryName" = 'Rehabilitation Equipment'
      AND c."ParentCategoryId" = (
        SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Physiotherapy & Rehabilitation' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1
      )
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Home Diagnostic Devices',
       (SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Homecare' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1),
       NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c
    WHERE c."CategoryName" = 'Home Diagnostic Devices'
      AND c."ParentCategoryId" = (
        SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Homecare' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1
      )
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Respiratory Devices',
       (SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Respiratory Care' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1),
       NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c
    WHERE c."CategoryName" = 'Respiratory Devices'
      AND c."ParentCategoryId" = (
        SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Respiratory Care' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1
      )
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Cold Boxes & Ice Packs',
       (SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Cold Chain & Vaccine Equipment' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1),
       NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c
    WHERE c."CategoryName" = 'Cold Boxes & Ice Packs'
      AND c."ParentCategoryId" = (
        SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Cold Chain & Vaccine Equipment' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1
      )
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Cardiac Monitoring Equipment',
       (SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Cardiology' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1),
       NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c
    WHERE c."CategoryName" = 'Cardiac Monitoring Equipment'
      AND c."ParentCategoryId" = (
        SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Cardiology' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1
      )
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Neurological Equipment',
       (SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Neurology' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1),
       NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c
    WHERE c."CategoryName" = 'Neurological Equipment'
      AND c."ParentCategoryId" = (
        SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Neurology' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1
      )
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Blood Collection & Phlebotomy',
       (SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Specimen Collection' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1),
       NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c
    WHERE c."CategoryName" = 'Blood Collection & Phlebotomy'
      AND c."ParentCategoryId" = (
        SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Specimen Collection' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1
      )
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Swabs & Specimen Containers',
       (SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Specimen Collection' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1),
       NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c
    WHERE c."CategoryName" = 'Swabs & Specimen Containers'
      AND c."ParentCategoryId" = (
        SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Specimen Collection' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1
      )
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'Trolleys & Storage',
       (SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Hospital Trolleys & Storage' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1),
       NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c
    WHERE c."CategoryName" = 'Trolleys & Storage'
      AND c."ParentCategoryId" = (
        SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'Hospital Trolleys & Storage' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1
      )
);
INSERT INTO "Categories" ("CategoryName","ParentCategoryId","Description","IsActive","CreatedDate")
SELECT 'General Hospital Supplies',
       (SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'General Hospital Supplies' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1),
       NULL, TRUE, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM "Categories" c
    WHERE c."CategoryName" = 'General Hospital Supplies'
      AND c."ParentCategoryId" = (
        SELECT "CategoryId" FROM "Categories"
        WHERE "CategoryName" = 'General Hospital Supplies' AND "ParentCategoryId" IS NULL
        ORDER BY "CategoryId" LIMIT 1
      )
);

-- Import catalogue products. Existing rows with the same MEV code are left unchanged.
WITH source_products("ProductCode","ProductName","PackSize","ParentCategory","SubCategory","RequiresBatchTracking") AS (
    VALUES
        ('MEV-001', 'Bin liner Black', '1x50', 'Waste Management', 'Waste Bins & Liners', TRUE),
        ('MEV-002', 'Bin liner Yellow', '1x50', 'Waste Management', 'Waste Bins & Liners', TRUE),
        ('MEV-003', 'Bin liner Red', '1x50', 'Waste Management', 'Waste Bins & Liners', TRUE),
        ('MEV-004', 'Waste bin red', '1x1', 'Waste Management', 'Waste Bins & Liners', FALSE),
        ('MEV-005', 'waste bin yellow', '1x1', 'Waste Management', 'Waste Bins & Liners', FALSE),
        ('MEV-006', 'Waste bin black', '1x1', 'Waste Management', 'Waste Bins & Liners', FALSE),
        ('MEV-007', 'Sharps boxes', '1x1', 'Waste Management', 'Sharps Disposal', TRUE),
        ('MEV-008', 'Urine containers', '1x100', 'Laboratory Consumables', 'Sample Handling', TRUE),
        ('MEV-009', 'Stool containers', '1x100', 'Laboratory Consumables', 'Sample Handling', TRUE),
        ('MEV-010', 'sputum mugs carton', '10x100', 'Laboratory Consumables', 'Sample Handling', TRUE),
        ('MEV-011', 'Sputum mugs', '1x100', 'Laboratory Consumables', 'Sample Handling', TRUE),
        ('MEV-012', 'Falcon tubes 15mL', '1x50', 'Laboratory Consumables', 'Sample Handling', TRUE),
        ('MEV-013', 'Falcon tubes 50mL', '1x25', 'Laboratory Consumables', 'Sample Handling', TRUE),
        ('MEV-014', '1000 uL pipette tips with filter', '1x96', 'Laboratory Consumables', 'Sample Handling', TRUE),
        ('MEV-015', '200 uL pipette tips with filters', '1x96', 'Laboratory Consumables', 'Sample Handling', TRUE),
        ('MEV-016', '10 uL pipette tips with filters', '1x96', 'Laboratory Consumables', 'Sample Handling', TRUE),
        ('MEV-017', '20 uL pipette tips with filters', '1x96', 'Laboratory Consumables', 'Sample Handling', TRUE),
        ('MEV-018', 'PCR tubes 0.2ml', '1×1000', 'Laboratory Consumables', 'Sample Handling', TRUE),
        ('MEV-019', 'PCR tubes 0.1mL', '1x1000', 'Laboratory Consumables', 'Sample Handling', TRUE),
        ('MEV-020', 'spin column tubes', '1x250', 'Laboratory Consumables', 'Sample Handling', TRUE),
        ('MEV-021', 'eppendorf tubes 1.5ml', '1x500', 'Laboratory Consumables', 'Sample Handling', TRUE),
        ('MEV-022', 'Qiagen DNA extraction kit 250', '1×250', 'Laboratory Consumables', 'Sample Handling', TRUE),
        ('MEV-023', 'Qiagen RNA extraction kit (spin column tubes, sample collection tubes, extraction reagents )', '1x250', 'Laboratory Consumables', 'Sample Handling', TRUE),
        ('MEV-024', 'Qiagen RNA extraction kit (spin column tubes, sample collection tubes, extraction reagents )', '1x100', 'Laboratory Consumables', 'Sample Handling', TRUE),
        ('MEV-025', 'cryo boxes plastic', '1x100', 'Laboratory Consumables', 'Sample Handling', TRUE),
        ('MEV-026', 'cryo boxes cardboard', '1x81', 'Laboratory Consumables', 'Sample Handling', TRUE),
        ('MEV-027', 'cryo boxes plastic', '1x81', 'Laboratory Consumables', 'Sample Handling', TRUE),
        ('MEV-028', 'cryo vials', '1x250', 'Laboratory Consumables', 'Sample Handling', TRUE),
        ('MEV-029', 'cryo vials', '1x100', 'Laboratory Consumables', 'Sample Handling', TRUE),
        ('MEV-030', 'cryo vials', '1x500', 'Laboratory Consumables', 'Sample Handling', TRUE),
        ('MEV-031', 'pasteur pipettes Lasec 3mL', '1x500', 'Laboratory Consumables', 'Sample Handling', TRUE),
        ('MEV-032', 'Standard Q Biosensor COVID-19 Ag', '1x25', 'Rapid Diagnostic & Test Kits', 'General Rapid Tests', TRUE),
        ('MEV-033', 'Abbott PanBio COVID-19 Ag', '1x25', 'Rapid Diagnostic & Test Kits', 'General Rapid Tests', TRUE),
        ('MEV-034', 'Accurate Typhoid IgG/IgM serum', '1x40', 'Rapid Diagnostic & Test Kits', 'General Rapid Tests', TRUE),
        ('MEV-035', 'Pal Typhoid Ag Stool', '1x25', 'Rapid Diagnostic & Test Kits', 'General Rapid Tests', TRUE),
        ('MEV-036', 'Pal H.Pylori Ab Serum/ Blood', '1x50', 'Rapid Diagnostic & Test Kits', 'General Rapid Tests', TRUE),
        ('MEV-037', 'Pal H.Pylori Ag Stool', '1x25', 'Rapid Diagnostic & Test Kits', 'General Rapid Tests', TRUE),
        ('MEV-038', 'Pal Hepatitis B', '1x50', 'Rapid Diagnostic & Test Kits', 'General Rapid Tests', TRUE),
        ('MEV-039', 'Accurate HBsAg', '1x50', 'Rapid Diagnostic & Test Kits', 'General Rapid Tests', TRUE),
        ('MEV-040', 'Pal Hepatitis A', '1x40', 'Rapid Diagnostic & Test Kits', 'General Rapid Tests', TRUE),
        ('MEV-041', 'Whole power HCV Antibody test strip (WB/Serum/Plasma)', '1x50', 'Rapid Diagnostic & Test Kits', 'General Rapid Tests', TRUE),
        ('MEV-042', 'Pal Hepatitis C', '1x50', 'Rapid Diagnostic & Test Kits', 'General Rapid Tests', TRUE),
        ('MEV-043', 'Pal Syphilis', '1x50', 'Rapid Diagnostic & Test Kits', 'General Rapid Tests', TRUE),
        ('MEV-044', 'Pal HCG strips urine', '1x50', 'Rapid Diagnostic & Test Kits', 'General Rapid Tests', TRUE),
        ('MEV-045', 'HCG serum strips', '1x50', 'Rapid Diagnostic & Test Kits', 'General Rapid Tests', TRUE),
        ('MEV-046', 'Accurate Syphilis', '1x50', 'Rapid Diagnostic & Test Kits', 'General Rapid Tests', TRUE),
        ('MEV-047', 'Abbott HIV/Syphilis Duo', '1x25', 'Rapid Diagnostic & Test Kits', 'General Rapid Tests', TRUE),
        ('MEV-048', 'HIV 1/2 Determine Ab', '1x100', 'Rapid Diagnostic & Test Kits', 'General Rapid Tests', TRUE),
        ('MEV-049', 'Hiv 1/2 Statpak', '1x20', 'Rapid Diagnostic & Test Kits', 'General Rapid Tests', TRUE),
        ('MEV-050', 'HIV 1/2 SD Bioline', '1x25', 'Rapid Diagnostic & Test Kits', 'General Rapid Tests', TRUE),
        ('MEV-051', 'First Response Malaria p.f Ag', '1x25', 'Rapid Diagnostic & Test Kits', 'Malaria Tests', TRUE),
        ('MEV-052', 'SD Malaria p.f pan', '1x25', 'Rapid Diagnostic & Test Kits', 'Malaria Tests', TRUE),
        ('MEV-053', 'Abbott malaria p.f', '1x25', 'Rapid Diagnostic & Test Kits', 'Malaria Tests', TRUE),
        ('MEV-054', 'Abbott malaria p.f/pan', '1x25', 'Rapid Diagnostic & Test Kits', 'Malaria Tests', TRUE),
        ('MEV-055', 'Brucella Agglutination Test (BAT) reagent Cypress', '1x5 mL for 100 tests', 'Laboratory Reagents', 'Serology & Immunology Reagents', TRUE),
        ('MEV-056', 'Rheumatoid factor Reagent Atlas', '1x5mL for 100 tests', 'Laboratory Reagents', 'Serology & Immunology Reagents', TRUE),
        ('MEV-057', 'CRP reagent Atlas', '1x5mL', 'Laboratory Reagents', 'Serology & Immunology Reagents', TRUE),
        ('MEV-058', 'BD automatic lancets', '1x100', 'Medical Sundries', 'Diagnostic Consumables', TRUE),
        ('MEV-059', 'Egolance automatic lancets', '1x100', 'Medical Sundries', 'Diagnostic Consumables', TRUE),
        ('MEV-060', 'HSY urine H-10 urine strips', '1x100', 'Medical Sundries', 'Diagnostic Consumables', TRUE),
        ('MEV-061', 'Frosted glass slides', '1x50', 'Medical Sundries', 'Diagnostic Consumables', TRUE),
        ('MEV-062', 'Frosted glass slides', '1x100', 'Medical Sundries', 'Diagnostic Consumables', TRUE),
        ('MEV-063', 'PDMS Alcohol swabs', '1x100', 'Medical Sundries', 'Diagnostic Consumables', TRUE),
        ('MEV-064', 'PDMS Alcohol swabs', '1x200', 'Medical Sundries', 'Diagnostic Consumables', TRUE),
        ('MEV-065', 'BD EDTA purple top vacutainers 4mL', '1x100', 'Specimen Collection', 'Blood Collection & Phlebotomy', TRUE),
        ('MEV-066', 'BD EDTA Red top vacutainers 4mL', '1x100', 'Specimen Collection', 'Blood Collection & Phlebotomy', TRUE),
        ('MEV-067', 'Yellow top vacutainer SST 4mL', '1x100', 'Specimen Collection', 'Blood Collection & Phlebotomy', TRUE),
        ('MEV-068', 'Blue top vacutainer 4mL', '1x100', 'Specimen Collection', 'Blood Collection & Phlebotomy', TRUE),
        ('MEV-069', 'BD Green top vacutainer 4mL', '1x100', 'Specimen Collection', 'Blood Collection & Phlebotomy', TRUE),
        ('MEV-070', 'Grey top vacutainer 4mL', '1x100', 'Specimen Collection', 'Blood Collection & Phlebotomy', TRUE),
        ('MEV-071', 'Plasma Preparation tubes PPT 5mL', '1x100', 'Specimen Collection', 'Blood Collection & Phlebotomy', TRUE),
        ('MEV-072', 'BD Green top vacutainer 10mL', '1x100', 'Specimen Collection', 'Blood Collection & Phlebotomy', TRUE),
        ('MEV-073', 'BD Vacutainer needles gauge 21', '1x100', 'Specimen Collection', 'Blood Collection & Phlebotomy', TRUE),
        ('MEV-074', 'BD butterfly needles gauge 23', '1x50', 'Specimen Collection', 'Blood Collection & Phlebotomy', TRUE),
        ('MEV-075', 'BD butterfly needles gauge 21', '1x50', 'Specimen Collection', 'Blood Collection & Phlebotomy', TRUE),
        ('MEV-076', 'BD vacutainer eclipse blood collection needle with preattached holder', '1x100', 'Specimen Collection', 'Blood Collection & Phlebotomy', TRUE),
        ('MEV-077', 'BD vacutainer needle holders', '1x250', 'Specimen Collection', 'Blood Collection & Phlebotomy', TRUE),
        ('MEV-078', 'Syringes 2mL', '1x100', 'Disposable Medical Supplies', 'Syringes & Injection Supplies', TRUE),
        ('MEV-079', 'Syringes 5mL', '1x100', 'Disposable Medical Supplies', 'Syringes & Injection Supplies', TRUE),
        ('MEV-080', 'Syringes 10mL', '1x100', 'Disposable Medical Supplies', 'Syringes & Injection Supplies', TRUE),
        ('MEV-081', 'Insulin syringes with needles', '1x100', 'Disposable Medical Supplies', 'Syringes & Injection Supplies', TRUE),
        ('MEV-082', 'Clinical thermometer', '1x1', 'Patient Care', 'Examination & Vital Signs', FALSE),
        ('MEV-083', 'Stethoscope class 3 classic', '1x1', 'Patient Care', 'Examination & Vital Signs', FALSE),
        ('MEV-084', 'Stethoscope class 2 classic', '1x1', 'Patient Care', 'Examination & Vital Signs', FALSE),
        ('MEV-085', 'plasters', '1x100', 'Wound Care', 'Plasters & Dressings', TRUE),
        ('MEV-086', 'First Aid kits Small', '1x1', 'Emergency & First Aid', 'First Aid Kits', TRUE),
        ('MEV-087', 'First Aid kits medium', '1x1', 'Emergency & First Aid', 'First Aid Kits', TRUE),
        ('MEV-088', 'First Aid kits Large', '1x1', 'Emergency & First Aid', 'First Aid Kits', TRUE),
        ('MEV-089', 'powdered latex gloves', '1x100', 'PPE & Infection Prevention', 'Gloves', TRUE),
        ('MEV-090', 'Powder free latex gloves', '1x100', 'PPE & Infection Prevention', 'Gloves', TRUE),
        ('MEV-091', 'Powder free nitrile gloves', '1x100', 'PPE & Infection Prevention', 'Gloves', TRUE),
        ('MEV-092', 'Surgical gloves size 7.5', '1x50 pairs', 'PPE & Infection Prevention', 'Gloves', TRUE),
        ('MEV-093', 'GSC light green frame clear lens uncoated', '1x1', 'PPE & Infection Prevention', 'Protective Clothing & Eye Protection', FALSE),
        ('MEV-094', 'Disposable hospital gowns', '1x1 piece', 'PPE & Infection Prevention', 'Protective Clothing & Eye Protection', TRUE),
        ('MEV-095', 'Disposable lab coats White', '1x1 piece', 'PPE & Infection Prevention', 'Protective Clothing & Eye Protection', TRUE),
        ('MEV-096', 'Disposable lab coats Blue', '1x1 piece', 'PPE & Infection Prevention', 'Protective Clothing & Eye Protection', TRUE),
        ('MEV-097', 'Coverallas white/Blue', '1x1 piece', 'PPE & Infection Prevention', 'Protective Clothing & Eye Protection', TRUE),
        ('MEV-098', 'Reusable ordinary lab coats', '1x1 piece', 'PPE & Infection Prevention', 'Protective Clothing & Eye Protection', FALSE),
        ('MEV-099', 'Surgical masks blue/black', '1x50', 'PPE & Infection Prevention', 'Masks & Respiratory Protection', TRUE),
        ('MEV-100', 'KN95 Respiratory protection mask', '1x25', 'PPE & Infection Prevention', 'Masks & Respiratory Protection', TRUE),
        ('MEV-101', 'N95 green face masks', '1x20', 'PPE & Infection Prevention', 'Masks & Respiratory Protection', TRUE),
        ('MEV-102', 'Oncall glucometer', '1x1', 'Diagnostic Equipment', 'Diabetes Monitoring', FALSE),
        ('MEV-103', 'Oncall glucose strips', '1x50', 'Diagnostic Equipment', 'Diabetes Monitoring', TRUE),
        ('MEV-104', 'HIMEDIA SIM MEDIA M181-500G', '1x500g', 'Laboratory Reagents', 'Microbiology Culture Media', TRUE),
        ('MEV-105', 'HIMEDIA TRIPLE IRON AGAR M0231-500G', '1X500g', 'Laboratory Reagents', 'Microbiology Culture Media', TRUE),
        ('MEV-106', 'HIMEDIA MUELLER HINTON AGAR No.2 M0184-500G', '1X500g', 'Laboratory Reagents', 'Microbiology Culture Media', TRUE),
        ('MEV-107', 'HIMEDIA UREA AGAR BASE (CHRISTENSEN)(AUTOCLAVABLE) M112-500G', '1X500g', 'Laboratory Reagents', 'Microbiology Culture Media', TRUE),
        ('MEV-108', 'HIMEDIA SIMMONS CITRATE AGAR N099-500G', '1X500g', 'Laboratory Reagents', 'Microbiology Culture Media', TRUE),
        ('MEV-109', 'HIMEDIA/Oxoid MacConkey', '1X500g', 'Laboratory Reagents', 'Microbiology Culture Media', TRUE),
        ('MEV-110', 'HIMEDIA/Oxoid blood agar base', '1X500g', 'Laboratory Reagents', 'Microbiology Culture Media', TRUE),
        ('MEV-111', 'Plate count Agar', '1X500g', 'Laboratory Reagents', 'Microbiology Culture Media', TRUE),
        ('MEV-112', 'Buffered pepton water', '1X500g', 'Laboratory Reagents', 'Microbiology Culture Media', TRUE),
        ('MEV-113', 'Sabouraud dextrose agar', '1X500g', 'Laboratory Reagents', 'Microbiology Culture Media', TRUE),
        ('MEV-114', 'Brain heart infusion broth', '1X500g', 'Laboratory Reagents', 'Microbiology Culture Media', TRUE),
        ('MEV-115', 'Nutrient agar', '1X500g', 'Laboratory Reagents', 'Microbiology Culture Media', TRUE),
        ('MEV-116', 'Rappaport Vassiliadis soy broth', '1X500g', 'Laboratory Reagents', 'Microbiology Culture Media', TRUE),
        ('MEV-117', 'Drug discs', '1x10', 'Laboratory Reagents', 'Microbiology Culture Media', TRUE),
        ('MEV-118', 'Petri Dishes', '1x500', 'Laboratory Consumables', 'General Laboratory Consumables', TRUE),
        ('MEV-119', 'HVS swabs', '1x100', 'Specimen Collection', 'Swabs & Specimen Containers', TRUE),
        ('MEV-120', 'Nasal swabs', '1x100', 'Specimen Collection', 'Swabs & Specimen Containers', TRUE),
        ('MEV-121', 'Oral swabs', '1x100', 'Specimen Collection', 'Swabs & Specimen Containers', TRUE),
        ('MEV-122', 'Arm Electonic Blood pressure Monitor Model:x802 Golden quality', '1x1', 'Diagnostic Equipment', 'Basic Diagnostic Equipment', FALSE),
        ('MEV-123', 'XSZ microscope', '1x1', 'Laboratory Equipment', 'Microscopes & Optical Equipment', FALSE),
        ('MEV-124', 'Olympus CX23 microscope', '1x1', 'Laboratory Equipment', 'Microscopes & Optical Equipment', FALSE),
        ('MEV-125', 'Autoclave 35L', '1x1', 'Sterilization & Disinfection', 'Sterilizers & Autoclaves', FALSE),
        ('MEV-126', 'Centrifuge 8 buckets', '1x1', 'Laboratory Equipment', 'Laboratory Processing Equipment', FALSE),
        ('MEV-127', 'Centrifuge 48 buckets', '1x1', 'Laboratory Equipment', 'Laboratory Processing Equipment', FALSE),
        ('MEV-128', 'Laboratory oven 50L', '1x1', 'Laboratory Equipment', 'Laboratory Heating & Incubation', FALSE),
        ('MEV-129', 'Laboratory culture incubator 54L', '1x1', 'Laboratory Equipment', 'Laboratory Heating & Incubation', FALSE),
        ('MEV-130', 'Biobase Biosafety cabinet class 2', '1x1', 'Laboratory Equipment', 'Biosafety & Containment', FALSE),
        ('MEV-131', 'Weighing scale Analog', '1x1', 'Laboratory Equipment', 'Basic Laboratory Equipment', FALSE),
        ('MEV-132', 'Analytical weighing scale digital', '1x1', 'Laboratory Equipment', 'Basic Laboratory Equipment', FALSE),
        ('MEV-133', 'Vortex mixer', '1x1', 'Laboratory Equipment', 'Laboratory Mixing & Shaking', FALSE),
        ('MEV-134', 'Slide warmer electronic / heat block', '1x1', 'Laboratory Equipment', 'Laboratory Heating & Incubation', FALSE),
        ('MEV-135', 'Water bath', '1x1', 'Laboratory Equipment', 'Laboratory Heating & Incubation', FALSE),
        ('MEV-136', 'Single channel automatic pipettesMicro 1000uL, 200uL, 10uL, 50uL, 20uL made in china', '1x1', 'Laboratory Equipment', 'Laboratory Pipetting', FALSE),
        ('MEV-137', 'Single channel automatic pipettesMicro 1000uL, 200uL, 10uL, 50uL, 20uL made in Germany', '1x1', 'Laboratory Equipment', 'Laboratory Pipetting', FALSE),
        ('MEV-138', 'Microscope lens tissue cleaner', '1x100 sheets', 'Laboratory Consumables', 'General Laboratory Consumables', TRUE),
        ('MEV-139', 'Field Stains A 1L ready to use', '1x1L', 'Laboratory Reagents', 'Microscopy & Staining Reagents', TRUE),
        ('MEV-140', 'Field Stains B 1L ready to use', '1x1L', 'Laboratory Reagents', 'Microscopy & Staining Reagents', TRUE),
        ('MEV-141', 'Blood group Antisera set A,B,AB, and D', '1x4 each', 'Laboratory Reagents', 'Serology & Immunology Reagents', TRUE),
        ('MEV-142', 'Blood grouping tile', '1x1', 'Laboratory Consumables', 'General Laboratory Consumables', FALSE),
        ('MEV-143', 'immersion oil', '1x1', 'Laboratory Reagents', 'Microscopy & Staining Reagents', TRUE),
        ('MEV-144', '70% Ethanol 20L', '1x20L', 'Laboratory Consumables', 'General Laboratory Consumables', TRUE),
        ('MEV-145', 'KJIK 5L', '1x5L', 'Laboratory Consumables', 'General Laboratory Consumables', TRUE),
        ('MEV-146', 'Parafilm', '1x1', 'Laboratory Consumables', 'General Laboratory Consumables', TRUE),
        ('MEV-147', 'Cotton wool 500g', '1x1', 'Laboratory Consumables', 'General Laboratory Consumables', TRUE),
        ('MEV-148', 'Paper towel', '1x2', 'Laboratory Consumables', 'General Laboratory Consumables', TRUE),
        ('MEV-149', 'Distilled water', '1x20L', 'Laboratory Consumables', 'General Laboratory Consumables', TRUE),
        ('MEV-150', 'Tourniquet', '1x1', 'Specimen Collection', 'Blood Collection & Phlebotomy', FALSE),
        ('MEV-151', 'Wheel chair', '1x1', 'Patient Care', 'Mobility & Patient Support', FALSE),
        ('MEV-152', 'Wheel chair with reusable under bucket', '1x1', 'Patient Care', 'Mobility & Patient Support', FALSE),
        ('MEV-153', 'Electric wheel chair', '1x1', 'Patient Care', 'Mobility & Patient Support', FALSE),
        ('MEV-154', 'Pulse oximeter', '1x1', 'Diagnostic Equipment', 'Basic Diagnostic Equipment', FALSE),
        ('MEV-155', 'Hemocue 301 HB machine', '1x1', 'Diagnostic Equipment', 'Hematology Analysers', FALSE),
        ('MEV-156', 'Hemocue 301 HB cuvettes', '1x50 tests in a tin', 'Laboratory Consumables', 'Sample Handling', TRUE),
        ('MEV-157', 'Hemocue 301 HB cuvettes box', '1x50x4', 'Laboratory Consumables', 'Sample Handling', TRUE),
        ('MEV-158', 'Hemocue 201 HB machine', '1x1', 'Diagnostic Equipment', 'Hematology Analysers', FALSE),
        ('MEV-159', 'Hemocue 201 HB cuvettes', '1x50 tests in a tin', 'Laboratory Consumables', 'Sample Handling', TRUE),
        ('MEV-160', 'Hemocue 201 HB cuvettes box', '1x50x4', 'Laboratory Consumables', 'Sample Handling', TRUE),
        ('MEV-161', 'CBC machine 3 part', '1x1', 'Diagnostic Equipment', 'Hematology Analysers', FALSE),
        ('MEV-162', 'CBC machine 5 part', '1x1', 'Diagnostic Equipment', 'Hematology Analysers', FALSE),
        ('MEV-163', 'mobile lab stools for microscopy', '1x1', 'Medical Furniture', 'Laboratory Furniture', FALSE),
        ('MEV-164', 'cool box 4.5L', '1x1', 'Cold Chain & Vaccine Equipment', 'Cold Boxes & Ice Packs', FALSE),
        ('MEV-165', 'Cool box 12L', '1x1', 'Cold Chain & Vaccine Equipment', 'Cold Boxes & Ice Packs', FALSE),
        ('MEV-166', 'Cool box 15L', '1x1', 'Cold Chain & Vaccine Equipment', 'Cold Boxes & Ice Packs', FALSE),
        ('MEV-167', 'Ice packs', '1x1', 'Cold Chain & Vaccine Equipment', 'Cold Boxes & Ice Packs', FALSE)
),
resolved AS (
    SELECT
        sp."ProductCode",
        sp."ProductName",
        sp."PackSize",
        sp."RequiresBatchTracking",
        c."CategoryId"
    FROM source_products sp
    JOIN "Categories" c
      ON c."CategoryName" = sp."SubCategory"
     AND c."ParentCategoryId" = (
         SELECT p."CategoryId"
         FROM "Categories" p
         WHERE p."CategoryName" = sp."ParentCategory"
           AND p."ParentCategoryId" IS NULL
         ORDER BY p."CategoryId"
         LIMIT 1
     )
)
INSERT INTO "Products"
(
    "ProductCode",
    "ProductName",
    "CategoryId",
    "BrandId",
    "Description",
    "Specifications",
    "UnitOfMeasure",
    "PackSize",
    "SellingPrice",
    "SalePrice",
    "SaleStartDate",
    "SaleEndDate",
    "Availability",
    "ReorderLevel",
    "RequiresBatchTracking",
    "IsActive",
    "CreatedDate",
    "ModifiedDate"
)
SELECT
    r."ProductCode",
    r."ProductName",
    r."CategoryId",
    NULL,
    NULL,
    NULL,
    NULL,
    r."PackSize",
    NULL,
    NULL,
    NULL,
    NULL,
    'AvailableOnRequest',
    0,
    r."RequiresBatchTracking",
    TRUE,
    CURRENT_TIMESTAMP,
    NULL
FROM resolved r
WHERE NOT EXISTS (
    SELECT 1 FROM "Products" p
    WHERE p."ProductCode" = r."ProductCode"
);

-- Link each imported product to its matching local PNG.
-- This is idempotent and does not delete or replace existing product images.
INSERT INTO "ProductImages" ("ProductId", "ImageUrl", "IsPrimary", "DisplayOrder")
SELECT
    p."ProductId",
    '/images/products/' || p."ProductCode" || '.png',
    TRUE,
    0
FROM "Products" p
WHERE p."ProductCode" LIKE 'MEV-%'
  AND NOT EXISTS (
      SELECT 1
      FROM "ProductImages" pi
      WHERE pi."ProductId" = p."ProductId"
        AND pi."ImageUrl" = '/images/products/' || p."ProductCode" || '.png'
  );

COMMIT;

-- Verification
SELECT
    COUNT(*) AS imported_catalogue_products
FROM "Products"
WHERE "ProductCode" LIKE 'MEV-%';

SELECT
    COUNT(*) AS catalogue_product_images
FROM "ProductImages" pi
JOIN "Products" p ON p."ProductId" = pi."ProductId"
WHERE p."ProductCode" LIKE 'MEV-%';

SELECT
    COUNT(*) AS products_missing_images
FROM "Products" p
WHERE p."ProductCode" LIKE 'MEV-%'
  AND NOT EXISTS (
      SELECT 1
      FROM "ProductImages" pi
      WHERE pi."ProductId" = p."ProductId"
        AND pi."ImageUrl" = '/images/products/' || p."ProductCode" || '.png'
  );

SELECT
    COUNT(*) AS active_catalogue_categories
FROM "Categories"
WHERE "IsActive" = TRUE;
