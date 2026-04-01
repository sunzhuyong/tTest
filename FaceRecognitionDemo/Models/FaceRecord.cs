using System;

namespace FaceRecognitionDemo.Models
{
    /// <summary>
    /// 人脸记录数据模型
    ///
    /// 类似于 Java 的:
    /// - 实体类 (Entity / Model)
    /// - POJO (Plain Old Java Object)
    /// - DTO (Data Transfer Object)
    ///
    /// C# 特性:
    /// - 自动属性 (Auto Property): public int Id { get; set; }
    ///   等同于 Java 的 private int id; public int getId(); public void setId(int id);
    /// - 可空引用类型: string.Empty 表示空字符串
    /// </summary>
    public class FaceRecord
    {
        /// <summary>
        /// 主键 ID - 自增
        /// 类似于 Java: @Id @GeneratedValue
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 姓名
        /// 类似于 Java: private String name;
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 人脸图片保存路径
        /// 类似于 Java: private String faceImagePath;
        /// </summary>
        public string FaceImagePath { get; set; } = string.Empty;

        /// <summary>
        /// 注册时间
        /// 类似于 Java: private LocalDateTime registerTime;
        /// </summary>
        public DateTime RegisterTime { get; set; }
    }
}
