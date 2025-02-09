using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FMR.Core.Collections
{
#pragma warning disable CS8625
    /// <summary>
    /// 表示一个单链表.
    /// </summary>
    /// <typeparam name="T">泛型参数.</typeparam>
    public class ForwardLinkedList<T> : IEnumerable<T>, IEnumerable
    {
        internal class ForwardLinkedListNode
        {
#pragma warning disable CS8601 // 引用类型赋值可能为 null。
            public T Item = default;
#pragma warning restore CS8601 // 引用类型赋值可能为 null。
#pragma warning disable CS8618
            public ForwardLinkedListNode Next;
#pragma warning restore CS8601
        }

        /// <summary>
        /// 表示单列表的迭代器.
        /// </summary>
        public class Enumerator(ForwardLinkedList<T> initLinkedList) : IEnumerator<T>, IEnumerator, IDisposable
        {
            private ForwardLinkedList<T> forwardList = initLinkedList;
            private ForwardLinkedListNode currentNode = initLinkedList.Root;

            /// <summary>
            /// 获取当前元素.
            /// </summary>
            public T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    return currentNode.Item;
                }
            }

            object? IEnumerator.Current => null;

            /// <inheritdoc/>
            public void Dispose()
            {
                forwardList = null;
                GC.SuppressFinalize(this);
            }

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                if (currentNode.Next != null)
                {
                    currentNode = currentNode.Next;
                    return true;
                }

                return false;
            }

            /// <inheritdoc/>
            public void Reset()
            {
                currentNode = forwardList.Root;
            }
        }

        private readonly ForwardLinkedListNode Root = new();

        private ForwardLinkedListNode End;

        private long count;

        /// <summary>
        /// 获取指向首个元素的引用. 此方法没有安全检查.
        /// </summary>
        public ref T First
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return ref Root.Next.Item;
            }
        }

        /// <summary>
        /// 获取指向末尾元素的引用. 此方法不保证获得有效元素.
        /// </summary>
        public ref T Last
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return ref End.Item;
            }
        }

        /// <summary>
        /// 当前单链表持有的元素数.
        /// </summary>
        public long Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return count;
            }
        }

        /// <summary>
        /// 初始化一个空单链表.
        /// </summary>
        public ForwardLinkedList()
        {
            End = Root;
        }
        /// <summary>
        /// 向单链表的末尾添加一个元素.
        /// </summary>
        /// <param name="item">要添加的元素.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T item)
        {
            End.Next = new ForwardLinkedListNode();
            End = End.Next;
            End.Item = item;
            count++;
        }

        /// <summary>
        /// 移除头部元素.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveFirst()
        {
            if (Root != End)
            {
                if (Root.Next == End)
                {
                    End = Root;
                    End.Next = null;
                }
                else
                {
                    Root.Next = Root.Next.Next;
                }

                count--;
            }
        }

        /// <summary>
        /// 将第一个元素移至尾部.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MoveFirstNodeToEnd()
        {
            ForwardLinkedListNode next = Root.Next;
            Root.Next = next.Next;
            End.Next = next;
            End = End.Next;
            next.Next = null;
        }

        /// <summary>
        /// 移除第一个元素，并返回这一元素.
        /// </summary>
        /// <returns>首个元素.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Pop()
        {
            T item = Root.Next.Item;
            RemoveFirst();
            return item;
        }

        /// <summary>
        /// 清除所有元素.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            Root.Next = null;
            End = Root;
            count = 0L;
        }

        /// <summary>
        /// 判断当前链表是否包含元素.
        /// </summary>
        /// <returns>如果存在至少一个元素，返回<see langword="true"/>；否则返回<see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Any()
        {
            return Root.Next != null;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
#pragma warning disable CS8603
            return null;
#pragma warning restore CS80603
        }

        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }
    }
#pragma warning restore CS8625
}