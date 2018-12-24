using System;
using System.Collections;
using System.Collections.Generic;
using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine;

/// <summary>
/// (Serializable) A bidirectional dictionary that lets you find the value by key OR key by value.
/// </summary>
/// <typeparam name="TFirst">Domain</typeparam>
/// <typeparam name="TSecond">Codomain</typeparam>
[System.Serializable]
public class SerializableBijectiveDictionaryBase<TFirst, TSecond> : IEnumerable<KeyValuePair<TFirst, TSecond>>, UnityEngine.ISerializationCallbackReceiver {
    // Runtime dictionaries
    private Dictionary<TFirst, TSecond> firstToSecond;
    private Dictionary<TSecond, TFirst> secondToFirst;

    public SerializableBijectiveDictionaryBase () {
        firstToSecond = new Dictionary<TFirst, TSecond> ();
        secondToFirst = new Dictionary<TSecond, TFirst> ();
    }

    // Serialized array representation of dictionaries
    #region ISerializationCallbackReceiver
    // Only first to second is serialized

    [UnityEngine.SerializeField]
    private List<TFirst> _keyValues;

    [UnityEngine.SerializeField]
    private TFirst[] _keys;
    [UnityEngine.SerializeField]
    private TSecond[] _values;

    void UnityEngine.ISerializationCallbackReceiver.OnAfterDeserialize () {
        if (_keys != null && _values != null) {
            //Need to clear the dictionary
            if (firstToSecond == null) {
                firstToSecond = new Dictionary<TFirst, TSecond> (_keys.Length);
                secondToFirst = new Dictionary<TSecond, TFirst> (_keys.Length);
            } else {
                firstToSecond.Clear ();
                secondToFirst.Clear ();
            }

            for (int i = 0; i < _keys.Length && i < _values.Length; i++) {
                //This should only happen with reference type keys (Generic, Object, etc)
                if (_keys[i] == null) {
                    continue;
                }

                //Add the data to the dictionary. Value can be null so no special step is required
                firstToSecond[_keys[i]] = _values[i];
                secondToFirst[_values[i]] = _keys[i];

            }

        }

        _keys = null;
        _values = null;
    }

    void UnityEngine.ISerializationCallbackReceiver.OnBeforeSerialize () {
        if (firstToSecond == null || firstToSecond.Count == 0) {
            //Dictionary is empty, erase data
            _keys = null;
            _values = null;
        } else {
            //Initialize arrays
            int cnt = firstToSecond.Count;
            _keys = new TFirst[cnt];
            _values = new TSecond[cnt];

            int i = 0;
            var e = firstToSecond.GetEnumerator ();
            while (e.MoveNext ()) {
                //Set the respective data from the dictionary
                _keys[i] = e.Current.Key;
                _values[i] = e.Current.Value;
                i++;
            }
        }
    }

    #endregion

    #region Exception throwing methods

    /// <summary>
    /// Tries to add the pair to the dictionary.
    /// Throws an exception if either element is already in the dictionary
    /// </summary>
    /// <param name="first"></param>
    /// <param name="second"></param>
    public void Add (TFirst first, TSecond second) {
        if (firstToSecond.ContainsKey (first) || secondToFirst.ContainsKey (second))
            throw new ArgumentException ("Duplicate first or second");

        firstToSecond.Add (first, second);
        secondToFirst.Add (second, first);
    }

    /// <summary>
    /// Find the TSecond corresponding to the TFirst first
    /// Throws an exception if first is not in the dictionary.
    /// </summary>
    /// <param name="first">the key to search for</param>
    /// <returns>the value corresponding to first</returns>
    public TSecond GetByFirst (TFirst first) {
        TSecond second;
        if (!firstToSecond.TryGetValue (first, out second))
            throw new ArgumentException ("first");

        return second;
    }

    /// <summary>
    /// Find the TFirst corresponing to the Second second.
    /// Throws an exception if second is not in the dictionary.
    /// </summary>
    /// <param name="second">the key to search for</param>
    /// <returns>the value corresponding to second</returns>
    public TFirst GetBySecond (TSecond second) {
        TFirst first;
        if (!secondToFirst.TryGetValue (second, out first))
            throw new ArgumentException ("second");

        return first;
    }

    /// <summary>
    /// Remove the record containing first.
    /// If first is not in the dictionary, throws an Exception.
    /// </summary>
    /// <param name="first">the key of the record to delete</param>
    public void RemoveByFirst (TFirst first) {
        TSecond second;
        if (!firstToSecond.TryGetValue (first, out second))
            throw new ArgumentException ("first");

        firstToSecond.Remove (first);
        secondToFirst.Remove (second);
    }

    /// <summary>
    /// Remove the record containing second.
    /// If second is not in the dictionary, throws an Exception.
    /// </summary>
    /// <param name="second">the key of the record to delete</param>
    public void RemoveBySecond (TSecond second) {
        TFirst first;
        if (!secondToFirst.TryGetValue (second, out first))
            throw new ArgumentException ("second");

        secondToFirst.Remove (second);
        firstToSecond.Remove (first);
    }

    #endregion

    #region Try methods

    /// <summary>
    /// Tries to add the pair to the dictionary.
    /// Returns false if either element is already in the dictionary        
    /// </summary>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <returns>true if successfully added, false if either element are already in the dictionary</returns>
    public bool TryAdd (TFirst first, TSecond second) {
        if (firstToSecond.ContainsKey (first) || secondToFirst.ContainsKey (second))
            return false;

        firstToSecond.Add (first, second);
        secondToFirst.Add (second, first);
        return true;
    }

    /// <summary>
    /// Find the TSecond corresponding to the TFirst first.
    /// Returns false if first is not in the dictionary.
    /// </summary>
    /// <param name="first">the key to search for</param>
    /// <param name="second">the corresponding value</param>
    /// <returns>true if first is in the dictionary, false otherwise</returns>
    public bool TryGetByFirst (TFirst first, out TSecond second) {
        return firstToSecond.TryGetValue (first, out second);
    }

    /// <summary>
    /// Find the TFirst corresponding to the TSecond second.
    /// Returns false if second is not in the dictionary.
    /// </summary>
    /// <param name="second">the key to search for</param>
    /// <param name="first">the corresponding value</param>
    /// <returns>true if second is in the dictionary, false otherwise</returns>
    public bool TryGetBySecond (TSecond second, out TFirst first) {
        return secondToFirst.TryGetValue (second, out first);
    }

    /// <summary>
    /// Remove the record containing first, if there is one.
    /// </summary>
    /// <param name="first"></param>
    /// <returns> If first is not in the dictionary, returns false, otherwise true</returns>
    public bool TryRemoveByFirst (TFirst first) {
        TSecond second;
        if (!firstToSecond.TryGetValue (first, out second))
            return false;

        firstToSecond.Remove (first);
        secondToFirst.Remove (second);
        return true;
    }

    /// <summary>
    /// Remove the record containing second, if there is one.
    /// </summary>
    /// <param name="second"></param>
    /// <returns> If second is not in the dictionary, returns false, otherwise true</returns>
    public bool TryRemoveBySecond (TSecond second) {
        TFirst first;
        if (!secondToFirst.TryGetValue (second, out first))
            return false;

        secondToFirst.Remove (second);
        firstToSecond.Remove (first);
        return true;
    }

    #endregion        

    /// <summary>
    /// The number of pairs stored in the dictionary
    /// </summary>
    public int Count {
        get { return firstToSecond.Count; }
    }

    public bool ContainsFirst (TFirst first) {
        return firstToSecond.ContainsKey (first);
    }

    public bool ContainsSecond (TSecond second) {
        return secondToFirst.ContainsKey (second);
    }

    /// <summary>
    /// Removes all items from the dictionary.
    /// </summary>
    public void Clear () {
        firstToSecond.Clear ();
        secondToFirst.Clear ();
    }

    IEnumerator IEnumerable.GetEnumerator () {
        return firstToSecond.GetEnumerator ();
    }

    public IEnumerator<KeyValuePair<TFirst, TSecond>> GetEnumerator () {
        return firstToSecond.GetEnumerator ();
    }
}