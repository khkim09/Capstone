import pandas as pd
import os
import json

input_dir = "DataSheet"  # CSV 파일이 있는 폴더
output_dir = "json_output"   # json 저장할 폴더

os.makedirs(output_dir, exist_ok=True)

# CSV 파일 목록을 가져옵니다
csv_files = [f for f in os.listdir(input_dir) if f.lower().endswith('.csv')]

if not csv_files:
    print("CSV 파일을 찾을 수 없습니다.")
    exit(1)

print(f"총 {len(csv_files)}개의 CSV 파일을 찾았습니다.")

for filename in csv_files:
    try:
        filepath = os.path.join(input_dir, filename)
        print(f"처리 중: {filepath}")
        
        # 파일 이름에서 확장자를 제외한 부분을 시트 이름으로 사용
        sheet_name = os.path.splitext(filename)[0]
        
        # UTF-8로 CSV 파일 읽기 (엔진은 필요 없음)
        df = pd.read_csv(filepath, encoding='utf-8')
        
        # 데이터를 인덱스 기준 딕셔너리로 변환
        data = df.to_dict(orient='index')
        
        # JSON 파일 경로 설정
        json_path = os.path.join(output_dir, f"{sheet_name}.json")
        
        # JSON으로 저장 (한글 및 유니코드 문자 지원)
        with open(json_path, 'w', encoding='utf-8') as f:
            json.dump(data, f, indent=2, ensure_ascii=False)
        
        print(f"JSON 생성 완료: {json_path}")
    
    except Exception as e:
        print(f"오류 발생 (파일: {filename}): {e}")

print("모든 CSV 파일의 변환이 완료되었습니다.")