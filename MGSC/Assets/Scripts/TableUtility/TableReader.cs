using System;
using UnityEngine;

namespace TableUtility
{

    public abstract class TableReader : ScriptableObject
    {
        public TableData Result;
        public Action OnReadingComplete;

        public abstract TableData ReadSheet();
    }

}