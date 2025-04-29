using System.Collections.Generic;
using UnityEngine;

public class SpecialEffectHandlerFactory
{
    private readonly Dictionary<SpecialEffectType, ISpecialEffectHandler> handlers = new();

    public SpecialEffectHandlerFactory()
    {
        handlers[SpecialEffectType.None] = null;
        handlers[SpecialEffectType.TriggerNextEvent] = new NextEventEffectHandler();
        handlers[SpecialEffectType.AddQuest] = new AddQuestEffectHandler();
        handlers[SpecialEffectType.UnlockItem] = new UnlockItemEffectHandler();
    }

    public ISpecialEffectHandler GetHandler(SpecialEffectType effectType)
    {
        if (handlers.TryGetValue(effectType, out var handler)) return handler;

        Debug.LogWarning($"처리기가 등록되지 않은 효과 유형: {effectType}");
        return null;
    }

    // 새로운 효과 유형 처리기 추가 메서드
    public void RegisterHandler(SpecialEffectType effectType, ISpecialEffectHandler handler)
    {
        handlers[effectType] = handler;
    }
}
