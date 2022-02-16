using System.Threading.Tasks;

namespace MobileApp.Common.Specifications.DataAccess {

    /// <summary>
    /// Class to write and read from a file.
    /// </summary>
    public interface IFileStorage {

        /// <summary>
        /// Reads a hole file.
        /// </summary>
        /// <param name="filePath">Filepath of the file to read.</param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The value of the TResult
        /// parameter contains the text of the file.
        /// </returns>
        Task<string> ReadAsString(string filePath);

        /// <summary>
        /// Writes a string to a file.
        /// Overrides an existing file.
        /// </summary>
        /// <param name="filePath">Filepath of the file.</param>
        /// <param name="text">Text to write to the file.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        Task WriteAllText(string filePath, string text);
    }
}
