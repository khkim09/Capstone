import pandas as pd
import os
import json

input_dir = "DataSheet"  # 엑셀 파일이 있는 폴더
output_dir = "json_output"   # json 저장할 폴더

os.makedirs(output_dir, exist_ok=True)

for filename in os.listdir(input_dir):
    if filename.endswith(".xlsx"):
        filepath = os.path.join(input_dir, filename)
        excel = pd.ExcelFile(filepath, engine="openpyxl")
        
        for sheet_name in excel.sheet_names[:10]:  # 처음 8개 시트만
            df = excel.parse(sheet_name)
            data = df.to_dict(orient='index')

            json_path = os.path.join(output_dir, f"{sheet_name}.json")
            with open(json_path, 'w', encoding='utf-8') as f:
                json.dump(data, f, indent=2, ensure_ascii=False)
