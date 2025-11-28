-- 005_add_event_counts.sql

-- 1. カウント用カラムの追加（デフォルト0）
ALTER TABLE events 
ADD COLUMN IF NOT EXISTS skill1_count INT DEFAULT 0,
ADD COLUMN IF NOT EXISTS skill2_count INT DEFAULT 0,
ADD COLUMN IF NOT EXISTS skill3_count INT DEFAULT 0,
ADD COLUMN IF NOT EXISTS enemy1_count INT DEFAULT 0,
ADD COLUMN IF NOT EXISTS enemy2_count INT DEFAULT 0,
ADD COLUMN IF NOT EXISTS enemy3_count INT DEFAULT 0;

-- 2. 既存データの移行（古い event_type カラムの値を新しいカウントカラムへ反映）
-- これを行わないと過去のイベントが「0回」として扱われてしまいます
UPDATE events SET skill1_count = 1 WHERE event_type = 'skill1';
UPDATE events SET skill2_count = 1 WHERE event_type = 'skill2';
UPDATE events SET skill3_count = 1 WHERE event_type = 'skill3';
UPDATE events SET enemy1_count = 1 WHERE event_type = 'enemy1';
UPDATE events SET enemy2_count = 1 WHERE event_type = 'enemy2';
UPDATE events SET enemy3_count = 1 WHERE event_type = 'enemy3';

-- 3. event_type カラムを NULL 許容に変更
-- 新しい INSERT クエリでは event_type を指定しないため、NOT NULL 制約を外す必要があります
ALTER TABLE events ALTER COLUMN event_type DROP NOT NULL;