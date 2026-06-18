public interface IAction
{
    public ActorSnapshot Snapshot { get; }
    
    /// <summary>
    /// 설정된 값에 따른 초기화
    /// </summary>
    public void Init(ActorData data);
    
    /// <summary>
    /// 갱신되는 정보에 대한 처리
    /// </summary>
    public void Apply(ActorSnapshot snapshot);
    
    /// <summary>
    /// Action에서 구현되는 동작
    /// </summary>
    // public void Process(ActorSnapshot snapshot);
    public void Process();
}