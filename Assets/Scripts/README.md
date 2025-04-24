
# Easy Save 3 (ES3) 직렬화 가이드

Unity 프로젝트에서 [Easy Save 3 (ES3)](https://assetstore.unity.com/packages/tools/input-management/easy-save-the-complete-save-load-asset-768) 사용 시, 사용자 정의 타입 직렬화 및 순환 참조 방지에 대한 정리입니다.

## 목차

- [1. Types 등록하기](#1-types-등록하기)
- [2. 순환 참조 방지 전략](#2-순환-참조-방지-전략)
- [3. 순환 참조 방지 예시 (불러오기)](#3-순환-참조-방지-예시-불러오기)
- [4. 팁 및 참고사항](#4-팁-및-참고사항)

---

## 1. Types 등록하기

1. Unity 상단 메뉴에서 `Window → Easy Save 3 → Types` 클릭
2. `Add Type`을 눌러 직렬화할 클래스 선택 (`PlayerData`, `Inventory`, `Item` 등)
3. 필요한 필드/프로퍼티 선택
4. `Auto Update` 체크 (권장)
5. `Apply Changes` 버튼 클릭

> ES3가 자동으로 `.es3types` 파일을 생성하며, 해당 타입의 저장/불러오기가 가능해집니다.

---

## 2. 순환 참조 방지 전략

### ❌ 잘못된 구조 (순환 참조 발생)

```csharp
[System.Serializable]
public class Parent
{
    public Child child;
}

[System.Serializable]
public class Child
{
    public Parent parent; // 순환 참조로 인해 직렬화 시 오류 발생 가능
}
```

### ✅ 권장 구조

```csharp
[System.Serializable]
public class Parent
{
    public Child child;
}

[System.Serializable]
public class Child
{
    [System.NonSerialized]
    public Parent parent;

    public void Initialize(Parent parent)
    {
        this.parent = parent;
    }
}
```

> 순환 참조가 있는 경우 부모는 자식을 직렬화하고, 자식은 부모를 직렬화하지 않습니다.  
> [System.NonSerialized] 필드를 붙이거나, ES3Types 설정 시에 체크 해제 합니다.  
> 자식을 불러온 후 부모 인스턴스를 수동으로 연결하는 방식으로 순환 참조를 방지합니다.

---

## 3. 순환 참조 방지 예시 (불러오기)

```csharp
public static int RestoreAllItemsToShip(Ship ship, string filename)
{
   /*
   아이템 역직렬화 코드
   */
   
   // 부모 창고 구하기
   torageRoomBase targetStorage = newItem.GetParentStorage();

   // 부모 창고에 아이템 추가
   if (targetStorage != null)
   {
      if (targetStorage.AddItem(newItem, newItem.GetGridPosition(), newItem.rotation))
          restoredCount++;
      else
          Debug.LogError("문제 있음");
      }
      else
      {
          GameObject.Destroy(newItem.gameObject); // 창고를 찾지 못한 경우 객체 삭제
      }   
   }
   return restoredCount;
}
```

---

## 4. 팁 및 참고사항

- `List`, `Dictionary`, 배열 등 컬렉션 타입도 Types에서 등록 가능
- `Auto Save` 기능을 사용하는 경우, 변경된 데이터만 자동 저장됨
- 에디터에서 `.es3types` 파일이 자동 생성되는지 확인 필수

---
