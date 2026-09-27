using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;

[InfoBox("This component is used to make a feature dependent on the BloodMachine. If the BloodMachine is not present, this feature will be disabled.")]
public class BloodMachineDependantFeature : MonoBehaviour
{
    enum BloodMachineDependency
    {
        RequiresBloodMachine,
        RequireNoBloodMachine
    }
    [SerializeField] BloodMachineDependency _bloodMachineDependency = BloodMachineDependency.RequiresBloodMachine;

    [SerializeField] List<GameObject> _dependantObjects;

    // Update is called once per frame
    void Update()
    {
        Refresh();
    }

    void Refresh()
    {
        if (_bloodMachineDependency == BloodMachineDependency.RequiresBloodMachine )
        {
            foreach (var obj in _dependantObjects)
            {
                obj.SetActive(Globals.Authorizations.AllowMakingDrinks);
            }
        }
        else if (_bloodMachineDependency == BloodMachineDependency.RequireNoBloodMachine)
        {
            foreach (var obj in _dependantObjects)
            {
                obj.SetActive(!Globals.Authorizations.AllowMakingDrinks);
            }
        }
    }
}
