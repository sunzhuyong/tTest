namespace CSharpLearningDemo.Models
{
    /// <summary>
    /// 狗类 - 演示【继承】和【多态】
    ///
    /// 继承：Dog 继承自 Animal
    /// - 使用 : Animal 表示继承关系
    /// - Dog 自动拥有 Animal 的所有公开成员 (Name, Age, Color, Speak, Eat 等)
    ///
    /// 多态：
    /// - Dog 重写了基类的 Speak() 方法
    /// - 运行时根据实际类型调用相应的方法
    /// </summary>
    public class Dog : Animal, IEat, ISpeak
    {
        // ==================== 自己的字段 ====================
        private string _breed;  // 品种

        // ==================== 属性 ====================
        public string Breed
        {
            get => _breed;
            set => _breed = value;
        }

        // ==================== 构造函数 ====================

        /// <summary>
        /// 狗的构造函数
        /// 使用 base() 调用父类构造函数
        /// </summary>
        public Dog(string name, int age, string color, string breed)
            : base(name, age, color)  // 调用父类构造函数初始化基类部分
        {
            _breed = breed;
        }

        // ==================== 重写父类方法 (多态) ====================

        /// <summary>
        /// 重写父类的 Speak 方法 - 演示【多态】
        /// override 关键字表示重写基类的虚方法
        /// 不同动物发出不同的声音
        /// </summary>
        public override void Speak()
        {
            Console.WriteLine($"{Name} 汪汪汪！");
        }

        /// <summary>
        /// 重写父类的 Eat 方法
        /// </summary>
        public override void Eat()
        {
            Console.WriteLine($"{Name} 正在吃狗粮...");
        }

        /// <summary>
        /// 重写 GetInfo 方法
        /// </summary>
        public override string GetInfo()
        {
            return $"🐕 {base.GetInfo()}, 品种: {_breed}";
        }

        // ==================== 实现接口 (IEat, ISpeak) ====================

        /// <summary>
        /// 实现 IEat 接口
        /// </summary>
        public void DoEat()
        {
            Console.WriteLine($"{Name} 正在吃骨头...");
        }

        public string GetFoodPreference()
        {
            return "喜欢啃骨头";
        }

        /// <summary>
        /// 实现 ISpeak 接口
        /// </summary>
        public void DoSpeak()
        {
            Speak();  // 调用重写后的方法
        }

        public string GetSoundDescription()
        {
            return "汪汪叫";
        }

        // ==================== 特有方法 ====================

        /// <summary>
        /// 狗特有的方法：看家
        /// </summary>
        public void Guard()
        {
            Console.WriteLine($"{Name} 正在看家！");
        }
    }
}
