using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bamboo.Utility;

namespace Bamboo.UI
{
    public class MenuManager : Singleton<MenuManager>
    {
        [SerializeField] private List<Menu> Menus;
        private Dictionary<string, GameObject> NameToGameObject;

        protected override void OnAwake()
        {
            _persistent = false;
        }

        private void Start()
        {
            Menus = new List<Menu>(Resources.FindObjectsOfTypeAll<Menu>());
            NameToGameObject = new Dictionary<string, GameObject>();
            foreach (Menu a in Menus)
            {
                a.OnAwake();
                NameToGameObject.Add(a.name, a.gameObject);
            }
        }

        public void AddMenu(Menu menu)
        {
            Menus.Add(menu);
            Debug.Log("Added a menu: " + menu.MenuName);
        }


        public void OpenMenu(string menuName)
        {
            bool menuFound = false;
            foreach (Menu a in Menus)
            {
                if (a.name == menuName)
                {
                    menuFound = true;
                    a.Open();
                }
            }

            if (!menuFound)
            {
                Debug.LogWarning("Attempted to open a menu: " + menuName + ", which currently does not exist!");
            }
        }

        public void OnlyOpenThisMenu(string menuName)
        {
            bool menuFound = false;
            foreach (Menu a in Menus)
            {
                if (a.name == menuName)
                {
                    menuFound = true;
                    a.Open();
                }
                else
                {
                    if (a.ignoreOpenOnlyOneCall)
                    {
                        continue;
                    }
                    a.Close();
                }
            }

            if (!menuFound)
            {
                Debug.LogWarning("Attempted to open a menu: " + menuName + ", which currently does not exist!");
            }
        }


        public void CloseMenu(string menuName)
        {
            bool menuFound = false;
            foreach (Menu a in Menus)
            {
                if (a.name == menuName)
                {
                    menuFound = true;
                    a.Close();
                }
            }
            if (!menuFound)
            {
                Debug.LogWarning("Attempted to close a menu: " + menuName + ", which currently does not exist!");
            }
        }
        public void CloseAllMenus()
        {
            foreach (Menu a in Menus)
            {
                if (a.ignoreCloseAllCall)
                {
                    continue;
                }
                a.Close();
            }
        }


        public GameObject getMenuGameObject(string menuName)
        {
            GameObject a;
            if(NameToGameObject.TryGetValue(menuName, out a))
            {
                Debug.LogWarning("Attempted to get gameobject of a menu: " + menuName + ", which currently does not exist!");
            }
            return a;
        }
    }
}