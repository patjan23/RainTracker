CREATE TABLE IF NOT EXISTS "RainData" (
    "Id" SERIAL PRIMARY KEY,
    "UserId" VARCHAR(255) NOT NULL,
    "Rain" BOOLEAN NOT NULL,
    "Timestamp" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS "IX_RainData_UserId" ON "RainData"("UserId");
CREATE INDEX IF NOT EXISTS "IX_RainData_Timestamp" ON "RainData"("Timestamp");
CREATE INDEX IF NOT EXISTS "IX_RainData_UserId_Timestamp" ON "RainData"("UserId", "Timestamp");
