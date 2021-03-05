using System;
using System.Collections.Generic;
using System.Text;

namespace core
{
    public class PriorityQueue<T> where T : IComparable<T>
    {
        List<T> _heap = new List<T>();
        public int Count { get { return _heap.Count; } }

        // O(log N)
        public void Push(T data)
        {
            // Heap의 맨 끝에 새로운 Data 삽입
            _heap.Add(data);

            int now = _heap.Count - 1;

            // 위치 set
            while (now > 0)
            {
                int next = (now - 1) / 2;

                // 현재 값이 다음 값보다 작을 경우 실패
                if (_heap[now].CompareTo(_heap[next]) < 0)
                    break;

                // 두 값 교체
                T temp = _heap[now];
                _heap[now] = _heap[next];
                _heap[next] = temp;

                // 검사 위치 이동
                now = next;
            }
        }

        // O(log N)
        public T Pop()
        {
            // 반환할 Data 저장
            T ret = _heap[0];

            // 마지막 Data를 Root로 이동
            int lastIndex = _heap.Count - 1;
            _heap[0] = _heap[lastIndex];
            _heap.RemoveAt(lastIndex);
            lastIndex--;

            // 역으로 검사
            int now = 0;
            while (true)
            {
                int left = 2 * now + 1;
                int right = 2 * now + 2;

                int next = now;
                // 왼쪽 값이 현재 값보다 크면 왼쪽으로 이동
                if (left <= lastIndex && _heap[next].CompareTo(_heap[left]) < 0)
                    next = left;

                // 오른쪽 값이 현재 값(왼쪽 이동 포함)보다 크면 오른쪽으로 이동
                if (right <= lastIndex && _heap[next].CompareTo(_heap[right]) < 0)
                    next = right;

                // 왼쪽/오른쪽 모두 현재값보다 작으면 종료
                if (next == now)
                    break;

                // 두 값을 교체
                T temp = _heap[now];
                _heap[now] = _heap[next];
                _heap[next] = temp;

                // 검사 위치 이동
                now = next;
            }

            return ret;
        }

        public T Peek()
        {
            if (_heap.Count == 0)
                return default(T);  // value type은 0, reference type은 null

            return _heap[0];
        }
    }
}