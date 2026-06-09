import os
import re

# Настройки путей
ASSETS_DIR = "Assets"
SETTINGS_DIR = "ProjectSettings"
BUILD_DIR = "Build" # Или "WebGL-Build", в зависимости от того, как вы назвали папку билда
REPORT_FILE = "YandexReport.txt"

# Ключевые слова для поиска в C# скриптах
KEYWORDS = {
    "SDK_Init": ["YG2.", "PluginYG2"],
    "Ads_Calls": ["InterstitialAdvShow", "RewardedAdvShow"],
    "Audio_Pause": ["OnApplicationFocus", "OnApplicationPause", "AudioListener.pause", "AudioListener.volume"],
    "Saves": ["PlayerPrefs", "YG2.saves", "LocalStorage", "SaveProgress"],
    "Input_Desktop": ["Input.GetAxis", "Input.GetKeyDown", "Input.GetKey"],
    "Input_Mobile": ["Input.touches", "Input.GetTouch", "EventSystem"],
    "Localization": ["YG2.lang", "switchLanguage"]
}

def get_dir_size(path):
    total_size = 0
    if os.path.exists(path):
        for dirpath, _, filenames in os.walk(path):
            for f in filenames:
                fp = os.path.join(dirpath, f)
                if not os.path.islink(fp):
                    total_size += os.path.getsize(fp)
    return total_size / (1024 * 1024) # в Мегабайтах

def scan_cs_scripts():
    results = {key: [] for key in KEYWORDS}
    
    if not os.path.exists(ASSETS_DIR):
        return {"Error": ["Папка Assets не найдена."]}

    for root, _, files in os.walk(ASSETS_DIR):
        for file in files:
            if file.endswith(".cs"):
                filepath = os.path.join(root, file)
                try:
                    with open(filepath, "r", encoding="utf-8") as f:
                        lines = f.readlines()
                        for i, line in enumerate(lines):
                            for category, words in KEYWORDS.items():
                                for word in words:
                                    if word in line and not line.strip().startswith("//"):
                                        clean_line = line.strip()[:100] # обрезаем слишком длинные строки
                                        results[category].append(f"[{file}:{i+1}] {clean_line}")
                except Exception as e:
                    pass
    return results

def scan_project_settings():
    settings_data = []
    settings_file = os.path.join(SETTINGS_DIR, "ProjectSettings.asset")
    
    if not os.path.exists(settings_file):
        return ["Файл ProjectSettings.asset не найден."]

    keys_to_find = [
        "defaultScreenHeight", "defaultScreenWidth", 
        "runInBackground", "WebGL", "colorSpace", 
        "m_BuildTarget", "m_ShowResolutionStartupDialog"
    ]

    try:
        with open(settings_file, "r", encoding="utf-8") as f:
            for line in f:
                for key in keys_to_find:
                    if key in line:
                        settings_data.append(line.strip())
    except:
        pass
    return settings_data

def generate_report():
    print("Начинаю сканирование проекта...")
    
    with open(REPORT_FILE, "w", encoding="utf-8") as report:
        report.write("=== ОТЧЕТ ДЛЯ ЯНДЕКС ИГР ===\n\n")
        
        # 1. Размер билда (Пункт 1.21)
        build_size = get_dir_size(BUILD_DIR)
        report.write(f"1. РАЗМЕР БИЛДА (Макс 100 МБ):\n")
        if build_size > 0:
            report.write(f"Размер папки {BUILD_DIR}: {build_size:.2f} МБ\n")
        else:
            report.write(f"Папка {BUILD_DIR} не найдена или пуста.\n")
        report.write("\n")

        # 2. Настройки проекта
        report.write("2. НАСТРОЙКИ UNITY PROJECT SETTINGS:\n")
        settings = scan_project_settings()
        for s in settings:
            report.write(f" - {s}\n")
        report.write("\n")

        # 3. Анализ скриптов
        report.write("3. АНАЛИЗ C# СКРИПТОВ:\n")
        script_results = scan_cs_scripts()
        
        for category, hits in script_results.items():
            report.write(f"\n--- Категория: {category} ---\n")
            if not hits:
                report.write("Ничего не найдено.\n")
            else:
                # Оставляем только первые 10 вхождений, чтобы не спамить
                for hit in hits[:10]:
                    report.write(f"{hit}\n")
                if len(hits) > 10:
                    report.write(f"... и еще {len(hits) - 10} совпадений.\n")

    print(f"Сканирование завершено! Результат сохранен в {REPORT_FILE}")

if __name__ == "__main__":
    generate_report()