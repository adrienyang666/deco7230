using UnityEngine; using UnityEngine.UI;
public class ButtonHandler:MonoBehaviour{public DebugRotator targetRotator;public Button toggleButton;void Start(){if(toggleButton)toggleButton.onClick.AddListener(ToggleRotation);}void ToggleRotation(){if(targetRotator)targetRotator.isRotating=!targetRotator.isRotating;}}
