using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericButton : MonoBehaviour
{
    public GameManager manager;
    public ViewController viewController;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public virtual void Awake()
    {
        viewController = (ViewController)FindObjectOfType(typeof(ViewController));
    }
}
