namespace CSharpLearningDemo.Models
{
    /// <summary>
    /// 猫类 - 演示【继承】和【多态】
    /// </summary>
    public class Cat : Animal, IEat, ISpeak
    {
        // ==================== 自己的字段 ====================
        private bool _isLazy;  // 是否懒惰

        // ==================== 属性 ====================
        public bool IsLazy
        {
            get => _isLazy;
            set => _isLazy = value;
        }

        // ==================== 构造函数 ====================

        public Cat(string name, int age, string color, bool isLazy = false)
            : base(name, age, color)
        {
            _isLazy = isLazy;
        }

        // ==================== 重写父类方法 (多态) ====================

        /// <summary>
        /// 重写 Speak 方法 - 演示【多态】
        /// 猫的叫声是"喵喵"
        /// </summary>
        public override void Speak()
        {
            Console.WriteLine($"{Name} 喵喵喵~");
        }

        /// <summary>
        /// 重写 Eat 方法
        /// </summary>
        public override void Eat()
        {
            Console.WriteLine($"{Name} 正在吃鱼罐头...");
        }

        /// <summary>
        /// 重写 GetInfo 方法
        /// </summary>
        public override string GetInfo()
        {
            string lazyStr = _isLazy ? "是" : "否";
            return $"🐱 {base.GetInfo()}, 懒: {lazyStr}";
        }

        // ==================== 实现接口 ====================

        public void DoEat()
        {
            Console.WriteLine($"{Name} 正在吃猫粮和小鱼干...");
        }

        public string GetFoodPreference()
        {
            return "喜欢鱼和小老鼠";
        }

        public void DoSpeak()
        {
            Speak();
        }

        public string GetSoundDescription()
        {
            return "喵喵叫";
        }

        // ==================== 特有方法 ====================

        /// <summary>
        /// 猫特有的方法：抓老鼠
        /// </summary>
        public void CatchMouse()
        {
            Console.WriteLine($"{Name} 正在抓老鼠！");
        }
    }
}
