using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class UICustomShadow : VisualElement
{
    private Label _mainText;
    private Label _shadowText;
    
    [UxmlAttribute]
    public Color textColor
    {
        get => _mainText.resolvedStyle.color;
        set => _mainText.style.color = value;
    }

    [UxmlAttribute]
    public string text
    {
        get => _mainText.text;
        set
        {
            _mainText.text = value;
            _shadowText.text = value;
        }
    }

    [UxmlAttribute]
    public Color shadowColor
    {
        get => _shadowText.resolvedStyle.color;
        set => _shadowText.style.color = value;
    }

    [UxmlAttribute]
    public Vector2 shadowOffset
    {
        get => new Vector2(_shadowText.resolvedStyle.translate.x, _shadowText.resolvedStyle.translate.y);
        set
        {
            _shadowText.style.translate = new Translate(value.x, value.y, 0);
        }
    }

    public UICustomShadow()
    {
        style.flexDirection = FlexDirection.Column;
        style.alignItems = Align.FlexStart;
        
        _shadowText = new Label();
        _shadowText.style.position = Position.Absolute;
        _shadowText.style.color = Color.black; 
        _shadowText.style.translate = new Translate(5, 5, 0); 
        
        _mainText = new Label();
        _mainText.style.color = Color.white;
        
        Add(_shadowText);
        Add(_mainText);
    }
}