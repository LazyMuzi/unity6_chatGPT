# CPC (Character-Player Connection)

AI 기반 NPC 대화와 관계 시스템을 갖춘 힐링 라이프 시뮬레이션 프로젝트입니다.

## Development Environment

- **Unity**: 6.0.6000.0.37f1
- **AI Model**: GPT-4o-mini (OpenAI API)

## Setup

이 프로젝트는 OpenAI API를 사용합니다.  
실행하려면 Unity 에디터에서 `AIManager` 오브젝트의 **Api Key** 필드에 본인의 API 키를 입력해야 합니다.

> API 키는 [OpenAI Platform](https://platform.openai.com/api-keys)에서 발급받을 수 있습니다.

## Dependencies

| Package | Description |
|---|---|
| Newtonsoft.Json | JSON 직렬화/역직렬화 |
| RestClient (Proyecto26) | Unity용 HTTP 클라이언트 |
| TextMeshPro | UI 텍스트 렌더링 |
| Cinemachine | 카메라 제어 |
| AI Navigation | NavMesh 경로 탐색 |

## License

This project is licensed under the [MIT License](LICENSE).
