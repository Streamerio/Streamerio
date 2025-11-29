using System;
using Common.UI.Animation;
using UnityEngine;

namespace Common.UI.Display.Window.Book
{
    [CreateAssetMenu(fileName = "BookTurnAnimationParamSO", menuName = "SO/UI/Animation/BookTurn")]
    public class BookTurnAnimationParamSO: UIAnimationComponentParamSO
    {
        public BookParam InitialBookParam;
        public BookParam[] BookParams;
    }

    [Serializable]
    public class BookParam
    {
        public Sprite page;
        public float posY;
        public float height;
    }
}