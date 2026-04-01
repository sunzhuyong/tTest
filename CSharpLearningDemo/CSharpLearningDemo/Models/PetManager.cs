namespace CSharpLearningDemo.Models
{
    /// <summary>
    /// 宠物管理器 - 演示【多态】的实际应用
    ///
    /// 使用 List<Animal> 存储不同类型的动物
    /// 遍历时自动调用各类型的 Speak() 方法
    /// 这就是多态的实际应用场景！
    /// </summary>
    public class PetManager
    {
        // ==================== 字段 ====================
        private List<Animal> _pets = new List<Animal>();

        // ==================== 属性 ====================

        /// <summary>
        /// 宠物数量
        /// </summary>
        public int Count => _pets.Count;

        // ==================== 方法 ====================

        /// <summary>
        /// 添加宠物 - 接受任何 Animal 及其子类
        /// 这是多态的体现：Dog、Cat、Bird 都可以传进来
        /// </summary>
        public void AddPet(Animal pet)
        {
            _pets.Add(pet);
        }

        /// <summary>
        /// 移除宠物
        /// </summary>
        public void RemovePet(int index)
        {
            if (index >= 0 && index < _pets.Count)
            {
                _pets.RemoveAt(index);
            }
        }

        /// <summary>
        /// 获取所有宠物
        /// </summary>
        public List<Animal> GetAllPets()
        {
            return new List<Animal>(_pets);
        }

        /// <summary>
        /// 让所有宠物发声 - 演示【多态】
        /// 关键点：这里只调用 Speak()，但运行时
        /// 会根据实际类型调用 Dog/Cat/Bird 的 Speak()
        /// </summary>
        public void MakeAllSpeak()
        {
            Console.WriteLine("\n===== 所有宠物叫声 =====");
            foreach (var pet in _pets)
            {
                // 这里是多态的体现！
                // 运行时根据 pet 的实际类型决定调用哪个 Speak()
                pet.Speak();
            }
        }

        /// <summary>
        /// 让所有宠物吃东西 - 演示【多态】
        /// </summary>
        public void FeedAll()
        {
            Console.WriteLine("\n===== 喂养所有宠物 =====");
            foreach (var pet in _pets)
            {
                pet.Eat();
            }
        }

        /// <summary>
        /// 显示所有宠物信息
        /// </summary>
        public void ShowAllInfo()
        {
            Console.WriteLine("\n===== 所有宠物信息 =====");
            for (int i = 0; i < _pets.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {_pets[i].GetInfo()}");
            }
        }

        /// <summary>
        /// 测试接口 - 演示接口的多态
        /// 如果宠物实现了 IEat 接口，就可以转型后调用
        /// </summary>
        public void TestInterfaces()
        {
            Console.WriteLine("\n===== 测试接口 =====");
            foreach (var pet in _pets)
            {
                // 使用 is 检查是否实现了接口
                if (pet is ISpeak speakable)
                {
                    Console.WriteLine($"{pet.Name} 的叫声: {speakable.GetSoundDescription()}");
                }

                if (pet is IEat eatable)
                {
                    Console.WriteLine($"{pet.Name} 喜欢: {eatable.GetFoodPreference()}");
                }

                // 鸟类还可以飞
                if (pet is IFlyable flyable)
                {
                    flyable.Fly();
                }

                Console.WriteLine();
            }
        }
    }
}
