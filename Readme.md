# Milky Road

<div align="center">

![Logo](https://github.com/user-attachments/assets/af28f060-19f0-4143-9ab4-52f722430e7d)


[![Documentation](https://img.shields.io/badge/docs-Doxygen-blue.svg)](https://khkim09.github.io/Capstone/)
![License](https://img.shields.io/badge/License-MIT-green.svg)

</div>

## 프로젝트 소개

TODO : 짧게 프로젝트 소개

## 시작하기

### 필수 조건

- Unity 6000.0.41f1

### 설치 방법

1. 이 저장소를 클론합니다:
   ```
   git clone https://github.com/khkim09/Capstone.git
   ```

2. Unity Hub에서 프로젝트를 엽니다.

3. 필요한 패키지가 자동으로 설치될 때까지 기다립니다.

## 개발 도구

### 함선 레이아웃 디자이너

프로젝트에는 함선 설계를 위한 웹 기반 도구가 포함되어 있습니다. 이 도구를 사용하면 비개발 인력이 함선 레이아웃을 시각적으로 설계하고 공유할 수 있습니다.

[함선 레이아웃 디자이너 열기](https://khkim09.github.io/Capstone/spaceship/)

<div align="center">
  
![레이아웃 디자이너 미리보기](https://github.com/user-attachments/assets/7937023b-bd81-4a7d-8adb-952c983cf4ad)

</div>

### 코드 문서

프로젝트의 코드 구조와 함수에 대한 상세 문서를 확인할 수 있습니다:

[코드 문서 보기](https://khkim09.github.io/Capstone/)

## 기여 방법

1. 이 저장소를 포크합니다.
2. 새로운 기능 브랜치를 생성합니다 (`git checkout -b feature/amazing-feature`).
3. 변경사항을 커밋합니다 (`git commit -m 'Add some amazing feature'`).
4. 브랜치에 푸시합니다 (`git push origin feature/amazing-feature`).
5. Pull Request를 생성합니다.

## 데이터 관리

프로젝트는 CSV 파일에서 JSON으로 변환되는 데이터 파이프라인을 사용합니다:

1. `DataSheet/` 폴더에 CSV 파일을 저장합니다.
2. GitHub Actions가 자동으로 CSV 파일을 JSON으로 변환합니다.
3. 변환된 JSON 파일은 `Assets/StreamingAssets/`에 저장됩니다.

**참고**: JSON 파일은 자동 생성되므로 직접 수정하지 마세요.

## 라이선스

이 프로젝트는 MIT 라이선스에 따라 라이선스가 부여됩니다.

## 팀원

TODO : 팀원 역할 적기, 깃허브 링크는 선택 사항

- [권혁재](링크) - 역할
- [김기현](링크) - 역할
- [손재민](링크) - 역할
- [조준영](링크) - 역할

## Acknowledgments

- [Easy Save](https://assetstore.unity.com/packages/tools/utilities/easy-save-the-complete-save-game-data-serializer-system-768) - 게임 데이터 저장 및 직렬화 시스템
- [Pixel Art Sci-Fi UI](https://assetstore.unity.com/packages/2d/gui/icons/pixel-art-sci-fi-ui-307172) - 게임 내 사용자 인터페이스 그래픽
- [2D Space Kit](https://assetstore.unity.com/packages/2d/environments/2d-space-kit-27662#content) - 게임 내 그래픽
- [PixelSpace](https://github.com/Deep-Fold/PixelSpace) - 게임 내 그래픽
