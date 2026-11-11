using System.ComponentModel;
namespace Studio.Core;

public sealed class TaskPreviewItem(TaskRule task,Func<string> language) : INotifyPropertyChanged
{
    private bool _selected=task.InitialSelection!=TaskInitialSelection.Unchecked;
    public string Description=>task.DescriptionFor(language());
    public bool IsSelected
    {
        get=>_selected;
        set { if(_selected==value) return; _selected=value; PropertyChanged?.Invoke(this,new(nameof(IsSelected))); }
    }
    public event PropertyChangedEventHandler? PropertyChanged;
    internal void RefreshDescription()=>PropertyChanged?.Invoke(this,new(nameof(Description)));
}
