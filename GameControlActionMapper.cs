using System.Reflection;
using Myra.Graphics2D.UI;
namespace GlobalConquest;

public class GameControlActionMapper
{
    Dictionary<string, string> IdToMethodName = new Dictionary<string, string>();
    Dictionary<string, object> IdToObject = new Dictionary<string, object>();
    Dictionary<string, Dictionary<int, string>> MenuIdToSelectedIndexMap = new Dictionary<string, Dictionary<int, string>>();
    public GameControlActionMapper()
    {
    }

    public void registerSelectedIndex(string menuId, int selectedIndex, string menuItemId)
    {
        if (!MenuIdToSelectedIndexMap.ContainsKey(menuId))
        {
            Dictionary<int, string> selectedIndexMap = new Dictionary<int, string>();
            MenuIdToSelectedIndexMap[menuId] = selectedIndexMap;
        }
        MenuIdToSelectedIndexMap[menuId][selectedIndex] = menuItemId;
    }

    public void registerControlMethod(string id, object o, string methodName)
    {
        IdToMethodName[id] = methodName;
        IdToObject[id] = o;
    }

    public void invoke(Widget widget)
    {
        Console.WriteLine("invoke(): enter: " + widget.GetType() + " " + widget.Id);
        String controlId = null;
        if ("Myra.Graphics2D.UI.VerticalMenu".Equals(widget.GetType().ToString()))
        {
            VerticalMenu verticalMenu = (VerticalMenu)widget;
            if (widget.Id != null && MenuIdToSelectedIndexMap.ContainsKey(widget.Id))
            {
                Console.WriteLine("invoke(): " + widget.Id + " in MenuIdToSelectedIndexMap");
                Dictionary<int, string> map = MenuIdToSelectedIndexMap[widget.Id];
                int selectedIndex = -1;
                if (verticalMenu.SelectedIndex != null)
                    selectedIndex = (int)verticalMenu.SelectedIndex;
                else if (verticalMenu.HoverIndex != null)
                    selectedIndex = (int)verticalMenu.HoverIndex;
                if (map.ContainsKey(selectedIndex))
                {
                    string menuItemId = map[selectedIndex];
                    controlId = menuItemId;
                    Console.WriteLine("invoke(): controlId=" + menuItemId);
                }
            }
        }
        if (controlId != null && IdToMethodName.ContainsKey(controlId) && IdToObject.ContainsKey(controlId))
        {
            Console.WriteLine("invoke(): " + controlId + " properly registered");
            string methodName = IdToMethodName[controlId];
            object o = IdToObject[controlId];
            Console.WriteLine("invoke(): method=" + o.GetType() + " " + methodName);
            MethodInfo method = o.GetType().GetMethod(methodName);
            object[] parameters = new object[] { };
            if (method != null)
                method.Invoke(o, parameters);
        }
    }

}
