using System;
using System.Collections;
using System.Collections.Generic;

namespace DSFramework.Collections
{
    /// <summary>
    ///     A shortcut for <see cref="TypeList{TBaseType}" /> to use object as base type.
    /// </summary>
    public class TypeList : TypeList<object>, ITypeList
    { }

    /// <summary>
    ///     Extends <see cref="List{T}" /> to add restriction a specific base type.
    /// </summary>
    /// <typeparam name="TBaseType">Base Type of <see cref="Type" />s in this list</typeparam>
    public class TypeList<TBaseType> : ITypeList<TBaseType>
    {
        private readonly List<Type> _typeList;

        /// <summary>
        ///     Gets the count.
        /// </summary>
        /// <value>The count.</value>
        public int Count => _typeList.Count;

        /// <summary>
        ///     Gets a value indicating whether this instance is read only.
        /// </summary>
        /// <value><c>true</c> if this instance is read only; otherwise, <c>false</c>.</value>
        public bool IsReadOnly => false;

        /// <summary>
        ///     Gets or sets the <see cref="Type" /> at the specified index.
        /// </summary>
        /// <param name="index">Index.</param>
        public Type this[int index]
        {
            get => _typeList[index];
            set
            {
                CheckType(value);
                _typeList[index] = value;
            }
        }

        /// <summary>
        ///     Creates a new <see cref="TypeList{T}" /> object.
        /// </summary>
        public TypeList()
        {
            _typeList = new List<Type>();
        }

        /// <inheritdoc />
        public void Add(Type item)
        {
            CheckType(item);
            _typeList.Add(item);
        }

        /// <inheritdoc />
        public bool Contains(Type item) => _typeList.Contains(item);

        /// <inheritdoc />
        public bool Remove(Type item) => _typeList.Remove(item);

        /// <inheritdoc />
        public void Clear() => _typeList.Clear();

        /// <inheritdoc />
        public void CopyTo(Type[] array, int arrayIndex) => _typeList.CopyTo(array, arrayIndex);

        /// <inheritdoc />
        public IEnumerator<Type> GetEnumerator() => _typeList.GetEnumerator();

        /// <inheritdoc />
        public void Insert(int index, Type item) => _typeList.Insert(index, item);

        /// <inheritdoc />
        public int IndexOf(Type item) => _typeList.IndexOf(item);

        /// <inheritdoc />
        public void RemoveAt(int index) => _typeList.RemoveAt(index);

        /// <inheritdoc />
        public void Add<T>() where T : TBaseType => _typeList.Add(typeof(T));

        /// <inheritdoc />
        public bool Contains<T>() where T : TBaseType => Contains(typeof(T));

        /// <inheritdoc />
        public void Remove<T>() where T : TBaseType => _typeList.Remove(typeof(T));

        private static void CheckType(Type item)
        {
            if (!typeof(TBaseType).IsAssignableFrom(item))
            {
                throw new ArgumentException("Given item is not type of " + typeof(TBaseType).AssemblyQualifiedName, nameof(item));
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => _typeList.GetEnumerator();
    }
}