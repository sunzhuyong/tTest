namespace CSharpLearningDemo.Models
{
    /// <summary>
    /// 鸟类 - 演示【继承】【多态】和【接口】
    /// 鸟类实现了 IFlyable 接口，可以飞行
    /// </summary>
    public class Bird : Animal, IEat, ISpeak, IFlyable
    {
        // ==================== 自己的字段 ====================
        private bool _canFly;
        private int _flightAltitude = 100;  // 默认飞行高度

        // ==================== 属性 ====================
        public bool CanFly
        {
            get => _canFly;
            set => _canFly = value;
        }

        /// <summary>
        /// 实现 IFlyable 接口的属性
        /// </summary>
        public int FlightAltitude
        {
            get => _flightAltitude;
            set => _flightAltitude = value;
        }

        // ==================== 构造函数 ====================

        public Bird(string name, int age, string color, bool canFly = true)
            : base(name, age, color)
        {
            _canFly = canFly;
        }

        // ==================== 重写父类方法 (多态) ====================

        /// <summary>
        /// 重写 Speak 方法 - 演示【多态】
        /// </summary>
        public override void Speak()
        {
            if (_canFly)
                Console.WriteLine($"{Name} 吱吱叫~");
            else
                Console.WriteLine($"{Name} 咕咕叫~");
        }

        /// <summary>
        /// 重写 Eat 方法
        /// </summary>
        public override void Eat()
        {
            Console.WriteLine($"{Name} 正在吃谷子...");
        }

        /// <summary>
        /// 重写 GetInfo 方法
        /// </summary>
        public override string GetInfo()
        {
            string flyStr = _canFly ? "能飞" : "不能飞";
            return $"🐦 {base.GetInfo()}, {flyStr}";
        }

        // ==================== 实现接口 ====================

        public void DoEat()
        {
            Console.WriteLine($"{Name} 正在吃谷物和虫子...");
        }

        public string GetFoodPreference()
        {
            return "喜欢谷物和虫子";
        }

        public void DoSpeak()
        {
            Speak();
        }

        public string GetSoundDescription()
        {
            return _canFly ? "吱吱叫" : "咕咕叫";
        }

        // ==================== 实现 IFlyable 接口 ====================

        /// <summary>
        /// 实现 IFlyable 接口的 Fly 方法
        /// </summary>
        public void Fly()
        {
            if (_canFly)
                Console.WriteLine($"{Name} 正在天空飞翔，高度 {_flightAltitude} 米！");
            else
                Console.WriteLine($"{Name} 不能飞，只能在地上走...");
        }

        // ==================== 特有方法 ====================

        /// <summary>
        /// 下蛋
        /// </summary>
        public void LayEgg()
        {
            Console.WriteLine($"{Name} 下了一个蛋！🥚");
        }
    }
}
