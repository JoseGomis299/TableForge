using TableForgeDemoFiles;
using UnityEngine;

public class TestMono : MonoBehaviour
{
    public Test1 test1;
    
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            (test1.intArray[0], test1.intArray[1]) = (test1.intArray[1], test1.intArray[0]);
        }
    }
}