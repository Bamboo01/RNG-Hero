using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Bamboo.UI
{
    public class Menu : MonoBehaviour
    {
        public bool ignoreOpenOnlyOneCall = false;
        public bool ignoreCloseAllCall = false;
        public string MenuName { get; private set; }

        public UnityEvent OnMenuOpen = new UnityEvent();
        public UnityEvent OnMenuClose = new UnityEvent();

        public virtual void OnAwake()
        {
            MenuName = gameObject.name;
        }

        public virtual void Open()
        {
            this.gameObject.SetActive(true);
            OnMenuOpen?.Invoke();
        }

        public virtual void Close()
        {
            this.gameObject.SetActive(false);
            OnMenuClose?.Invoke();
        }

        public GameObject GetObject()
        {
            return gameObject;
        }
    }

}
