namespace CSharpLearningDemo.Models
{
    /// <summary>
    /// 进食接口 - 演示【接口】
    ///
    /// 接口定义了一组行为的规范
    /// - 接口用 interface 关键字定义
    /// - 接口中的方法只有声明，没有实现
    /// - 类可以实现多个接口
    /// </summary>
    public interface IEat
    {
        /// <summary>
        /// 进食方法 - 接口定义
        /// </summary>
        void DoEat();

        /// <summary>
        /// 获取食物偏好
        /// </summary>
        string GetFoodPreference();
    }

    /// <summary>
    /// 叫声接口 - 演示【接口】
    /// </summary>
    public interface ISpeak
    {
        /// <summary>
        /// 发出叫声
        /// </summary>
        void DoSpeak();

        /// <summary>
        /// 获取叫声描述
        /// </summary>
        string GetSoundDescription();
    }

    /// <summary>
    /// 可飞行接口 - 演示【接口】
    /// 只有鸟类可以飞行，实现这个接口
    /// </summary>
    public interface IFlyable
    {
        /// <summary>
        /// 飞行方法
        /// </summary>
        void Fly();

        /// <summary>
        /// 飞行高度
        /// </summary>
        int FlightAltitude { get; }
    }

    /// <summary>
    /// 可游泳接口 - 演示【接口】
    /// </summary>
    public interface ISwimable
    {
        /// <summary>
        /// 游泳方法
        /// </summary>
        void Swim();
    }
}
