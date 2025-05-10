using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SpecialEffectType에 대응되는 핸들러를 생성하고 제공하는 팩토리 클래스입니다.
/// </summary>
public class SpecialEffectHandlerFactory
{
    private readonly Dictionary<SpecialEffectType, ISpecialEffectHandler> handlers = new();

    /// <summary>
    /// 생성자: 모든 기본 SpecialEffectType에 대해 핸들러를 등록합니다.
    /// </summary>
    public SpecialEffectHandlerFactory()
    {
        RegisterAllDefaultHandlers();
    }

    /// <summary>
    /// 모든 기본 핸들러들을 등록합니다.
    /// </summary>
    private void RegisterAllDefaultHandlers()
    {
        RegisterHandler(SpecialEffectType.None, null);
        RegisterHandler(SpecialEffectType.TriggerNextEvent, new EventNextEventEffectHandler());
        RegisterHandler(SpecialEffectType.AddQuest, new AddQuestEffectHandler());
        handlers[SpecialEffectType.RoomDamage] = new RoomDamageEffectHandler();
        handlers[SpecialEffectType.Battle] = new BattleEffectHandler();
    }

    /// <summary>
    /// 특정 SpecialEffectType에 해당하는 핸들러를 등록합니다.
    /// </summary>
    private void RegisterHandler(SpecialEffectType type, ISpecialEffectHandler handler)
    {
        if (handlers.ContainsKey(type))
        {
            Debug.LogWarning($"[SpecialEffectHandlerFactory] 이미 등록된 핸들러가 존재합니다: {type}");
            return;
        }

        handlers[type] = handler;
    }

    /// <summary>
    /// 특정 SpecialEffectType에 해당하는 핸들러를 반환합니다.
    /// </summary>
    public ISpecialEffectHandler GetHandler(SpecialEffectType type)
    {
        if (!handlers.ContainsKey(type))
        {
            Debug.LogWarning($"[SpecialEffectHandlerFactory] 등록되지 않은 핸들러 요청: {type}");
            return null;
        }

        return handlers[type];
    }
}
