namespace CSharpLearningDemo.Models
{
    /// <summary>
    /// 动物基类 - 演示【封装】
    ///
    /// 封装：将数据和操作封装在类中，对外提供公开的访问接口
    /// - Name 属性：封装了私有字段 _name
    /// - Age 属性：封装了私有字段 _age
    /// - 方法：封装了行为
    /// </summary>
    public class Animal
    {
        // ==================== 私有字段 (封装) ====================
        // private 修饰符表示只能在类内部访问
        // 这是封装的体现：隐藏内部实现细节
        private string _name;
        private int _age;
        private string _color;

        // ==================== 公开属性 (封装) ====================
        // 属性提供受控的访问方式，可以添加验证逻辑

        /// <summary>
        /// 动物名字 - 通过属性封装私有字段
        /// </summary>
        public string Name
        {
            get => _name;           // 读取时返回 _name
            set
            {
                // 可以添加验证逻辑
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("名字不能为空！");
                _name = value;
            }
        }

        /// <summary>
        /// 动物年龄 - 演示封装
        /// </summary>
        public int Age
        {
            get => _age;
            set
            {
                if (value < 0)
                    throw new ArgumentException("年龄不能为负数！");
                _age = value;
            }
        }

        /// <summary>
        /// 动物颜色
        /// </summary>
        public string Color
        {
            get => _color;
            set => _color = value;
        }

        // ==================== 构造函数 ====================

        /// <summary>
        /// 构造函数 - 初始化动物对象
        /// </summary>
        public Animal(string name, int age, string color)
        {
            Name = name;      // 使用属性，会触发验证
            Age = age;
            Color = color;
        }

        // ==================== 虚方法 (多态基础) ====================

        /// <summary>
        /// 动物叫声 - 虚方法，可以被子类重写
        /// virtual 关键字表示这个方法可以被 override 重写
        /// 这是实现【多态】的基础
        /// </summary>
        public virtual void Speak()
        {
            Console.WriteLine($"{_name} 发出了声音");
        }

        /// <summary>
        /// 动物吃东西
        /// </summary>
        public virtual void Eat()
        {
            Console.WriteLine($"{_name} 正在吃东西");
        }

        /// <summary>
        /// 获取动物信息 - 演示多态
        /// </summary>
        public virtual string GetInfo()
        {
            return $"名字: {_name}, 年龄: {_age}岁, 颜色: {_color}";
        }
    }
}
