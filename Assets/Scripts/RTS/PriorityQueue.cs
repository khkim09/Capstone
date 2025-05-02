using System.Collections.Generic;

/// <summary>
/// 간단한 우선순위 큐 구현 (A*에서 사용)
/// </summary>
/// <typeparam name="T"></typeparam>
public class PriorityQueue<T>
{
    private List<(T item, int priority)> elements = new();

    public int Count => elements.Count;

    public void Enqueue(T item, int priority)
    {
        elements.Add((item, priority));
    }

    public T Dequeue()
    {
        int bestIndex = 0;

        for (int i = 0; i < elements.Count; i++)
            if (elements[i].priority < elements[bestIndex].priority)
                bestIndex = i;

        T bestItem = elements[bestIndex].item;
        elements.RemoveAt(bestIndex);
        return bestItem;
    }
}
