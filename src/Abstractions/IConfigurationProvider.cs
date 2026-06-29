namespace CadIME.Abstractions
{
    /// <summary>
    /// 外置配置文件持久化层行为契约
    /// </summary>
    /// <typeparam name="TConfig">配置实体强类型</typeparam>
    public interface IConfigurationProvider<TConfig> where TConfig : class
    {
        /// <summary>
        /// 从盘区加载配置。如果文件损坏或缺失，必须回退至硬编码安全默认值。
        /// </summary>
        /// <returns>加载或回退后的有效配置对象</returns>
        TConfig Load();

        /// <summary>
        /// 将当前配置持久化写入盘区。
        /// </summary>
        /// <param name="config">待保存的有效配置实例</param>
        void Save(TConfig config);
    }
}
