import os
from google import genai

def main():
    # Gemini API 키 읽기
    gemini_api_key = os.getenv("GEMINI_API_KEY")
    if not gemini_api_key:
        raise Exception("GEMINI_API_KEY 환경변수가 설정되지 않았습니다.")

    # Gemini API 초기화
    genai.configure(api_key=gemini_api_key)

    # diff.txt 읽기
    with open('diff.txt', 'r', encoding='utf-8') as f:
        diff_output = f.read()

    # 프롬프트 텍스트 구성
    prompt_text = f"""
    다음은 깃허브 PR 에 올라온 수정된 코드들 입니다.
    
    Git diff를 분석하고 이 PR이 뭘 변경했는지 하기 Template으로 요약해주세요.  
        
     # Recommand
     - 이 PR의 제목을 추천해주세요
     
     # Summary
     - 무엇을, 왜 수정했는지 간단하게 요약합니다.
             
     # What's Changed
     - 핵심 변경 사항을 취합 및 요약해주세요, bullet point를 활용해 3줄 정도로 정리해주세요
               
     # Side Effects
    - 시스템에 어떤 문제 생길 수 있는지 짚어주세요.
    
    # Suggest
    - 이 PR에서 잘 처리된 부분은 무엇이고 잘 처리되지 않은 부분이 무엇인지 짚어주세요.
    
    ## 잘 처리된 부분
    - 예시
        - XXX 모듈의 책임 분리 → 유지보수성과 가독성 향상
        - 불필요한 의존성 제거 → 코드 간결성 증가
        - 인터페이스 분리 및 추상화 계층 도입 → 테스트 및 확장성 고려한 설계
        
    ## 잘 처리되지 않은 부분
    - "예시"에 맞춰서 여러 항목을 입력해도 좋습니다.
    - 예시
        ---    
        문제: 어떤 부분이 잘 처리되지 않았는지 명확히 서술합니다.
        개선안: 구체적으로 어떻게 개선하면 좋을지 제안합니다.
        참고 파일/위치: 수정이 필요한 코드의 위치나 파일 경로를 명시합니다.

        추천 등급: (P1 ~ P5 중 하나 선택)        
            P1: 꼭 반영해주세요 (Request Change)
            P2: 적극적으로 고려해주세요 (Request Change)
            P3: 웬만하면 반영해주세요 (Comment)
            P4: 반영해도 좋고 넘어가도 좋습니다 (Approve)
            P5: 그냥 사소한 의견입니다 (Approve)
            등급 사유: 왜 해당 등급을 부여했는지 논리적으로 설명합니다.                
        ---
    
    결과는 반드시 한국어로 번역해주세요.
    
    <git diff>{diff_output}</git diff>
    """

    # 모델 설정
    generation_config = {
        "temperature": 0.9,
        "top_p": 1,
        "top_k": 1,
        "max_output_tokens": 2048,
    }

    # Gemini 모델 생성
    model = genai.GenerativeModel('gemini-1.5-pro', generation_config=generation_config)
    response = model.generate_content(prompt_text)

    # 결과 파일로 저장
    with open('review_result.txt', 'w', encoding='utf-8') as f:
        f.write(response.text)

if __name__ == "__main__":
    main()