using UnityEngine;
using System.Collections;
using System.Diagnostics;
using System;

public class LaunchBTManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
        gameObject.SetActive(Environment.OSVersion.Version.Major == 10);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void LaunchBluetoothManager()
    {
        // Windows 10
        Process cmd = new Process();

        cmd.StartInfo.CreateNoWindow = true;
        cmd.StartInfo.UseShellExecute = false;
        cmd.StartInfo.FileName = "powershell.exe";
        cmd.StartInfo.Arguments = "start ms-settings:bluetooth";
        cmd.Start();

        // Untested Windows 8 Command:
        // control / name Microsoft.BluetoothDevices
    }

}
