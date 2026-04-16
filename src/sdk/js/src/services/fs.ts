import type * as grpc from '@grpc/grpc-js';
import { promisifyUnary } from '../grpc-call.js';
import type {
  AppendTextRequest,
  AppendTextResponse,
  CopyFileRequest,
  CopyFileResponse,
  CreateDirectoryRequest,
  CreateDirectoryResponse,
  DeleteDirectoryRequest,
  DeleteDirectoryResponse,
  DeleteFileRequest,
  DeleteFileResponse,
  ExistsRequest,
  ExistsResponse,
  FindExeShortcutsRequest,
  FindExeShortcutsResponse,
  GetItemInfoRequest,
  GetItemInfoResponse,
  ListDirectoryRequest,
  ListDirectoryResponse,
  MoveDirectoryRequest,
  MoveDirectoryResponse,
  MoveFileRequest,
  MoveFileResponse,
  ReadTextRequest,
  ReadTextResponse,
  SearchFilesRequest,
  SearchFilesResponse,
  WriteTextRequest,
  WriteTextResponse,
} from '../proto-types.js';

/**
 * 文件系统 gRPC 客户端（Proto 服务 `swg.fs.FsService`）。
 *
 * 提供文件读写、复制/移动/删除、目录操作、文件搜索等文件系统操作。
 * 所有方法均为无状态调用，线程安全。
 */
export class FsServiceClient {
  constructor(private readonly client: grpc.Client) {}

  /**
   * 读取文本文件的完整内容。
   *
   * @param request 请求参数：
   *   - `path`（string，必填）：文件路径
   *   - `encoding`（string，可选）：文本编码名称（如 `utf-8`、`gb2312`），为空使用默认编码
   * @returns 包含 `text`（string）：文件文本内容
   * @throws gRPC `InvalidArgument` — path 为空
   */
  readText(request: ReadTextRequest): Promise<ReadTextResponse> {
    return promisifyUnary<ReadTextRequest, ReadTextResponse>(this.client, 'readText', request);
  }

  /**
   * 将文本写入文件（覆盖已有内容）。
   *
   * @param request 请求参数：
   *   - `path`（string，必填）：文件路径
   *   - `text`（string，必填）：要写入的文本内容
   *   - `encoding`（string，可选）：文本编码名称
   * @returns 包含 `success`（boolean）：是否写入成功
   * @throws gRPC `InvalidArgument` — path 或 text 为空
   */
  writeText(request: WriteTextRequest): Promise<WriteTextResponse> {
    return promisifyUnary<WriteTextRequest, WriteTextResponse>(this.client, 'writeText', request);
  }

  /**
   * 将文本追加到文件末尾（文件不存在时自动创建）。
   *
   * @param request 请求参数：
   *   - `path`（string，必填）：文件路径
   *   - `text`（string，必填）：要追加的文本内容
   *   - `encoding`（string，可选）：文本编码名称
   * @returns 包含 `success`（boolean）：是否追加成功
   * @throws gRPC `InvalidArgument` — path 或 text 为空
   */
  appendText(request: AppendTextRequest): Promise<AppendTextResponse> {
    return promisifyUnary<AppendTextRequest, AppendTextResponse>(this.client, 'appendText', request);
  }

  /**
   * 复制文件到目标路径。
   *
   * @param request 请求参数：
   *   - `sourcePath`（string，必填）：源文件路径
   *   - `targetPath`（string，必填）：目标文件路径
   *   - `overwrite`（boolean）：是否覆盖已存在的目标文件
   * @returns 包含 `success`（boolean）：是否复制成功
   * @throws gRPC `InvalidArgument` — sourcePath 或 targetPath 为空
   */
  copyFile(request: CopyFileRequest): Promise<CopyFileResponse> {
    return promisifyUnary<CopyFileRequest, CopyFileResponse>(this.client, 'copyFile', request);
  }

  /**
   * 移动文件到目标路径。
   *
   * @param request 请求参数：
   *   - `sourcePath`（string，必填）：源文件路径
   *   - `targetPath`（string，必填）：目标文件路径
   *   - `overwrite`（boolean）：是否覆盖已存在的目标文件
   * @returns 包含 `success`（boolean）：是否移动成功
   * @throws gRPC `InvalidArgument` — sourcePath 或 targetPath 为空
   */
  moveFile(request: MoveFileRequest): Promise<MoveFileResponse> {
    return promisifyUnary<MoveFileRequest, MoveFileResponse>(this.client, 'moveFile', request);
  }

  /**
   * 删除指定文件。
   *
   * @param request 请求参数：
   *   - `path`（string，必填）：要删除的文件路径
   * @returns 包含 `success`（boolean）：是否删除成功
   * @throws gRPC `InvalidArgument` — path 为空
   */
  deleteFile(request: DeleteFileRequest): Promise<DeleteFileResponse> {
    return promisifyUnary<DeleteFileRequest, DeleteFileResponse>(this.client, 'deleteFile', request);
  }

  /**
   * 检查指定路径的文件或目录是否存在。
   *
   * @param request 请求参数：
   *   - `path`（string，必填）：要检查的路径
   * @returns 包含 `exists`（boolean）：路径是否存在
   * @throws gRPC `InvalidArgument` — path 为空
   */
  exists(request: ExistsRequest): Promise<ExistsResponse> {
    return promisifyUnary<ExistsRequest, ExistsResponse>(this.client, 'exists', request);
  }

  /**
   * 获取文件或目录的详细信息（大小、时间戳、属性等）。
   *
   * @param request 请求参数：
   *   - `path`（string，必填）：目标路径
   * @returns 包含以下字段：
   *   - `name`（string）：文件/目录名称
   *   - `fullPath`（string）：完整路径
   *   - `isDirectory`（boolean）：是否为目录
   *   - `hasSizeBytes` / `sizeBytes`（number）：文件大小字节数（目录无此字段）
   *   - `creationTimeUtc`（Timestamp）：创建时间（UTC）
   *   - `lastWriteTimeUtc`（Timestamp）：最后修改时间（UTC）
   *   - `lastAccessTimeUtc`（Timestamp）：最后访问时间（UTC）
   *   - `attributes`（string）：文件属性字符串
   * @throws gRPC `InvalidArgument` — path 为空
   */
  getItemInfo(request: GetItemInfoRequest): Promise<GetItemInfoResponse> {
    return promisifyUnary<GetItemInfoRequest, GetItemInfoResponse>(this.client, 'getItemInfo', request);
  }

  /**
   * 创建目录（递归创建所有不存在的父目录）。
   *
   * @param request 请求参数：
   *   - `path`（string，必填）：要创建的目录路径
   * @returns 包含 `success`（boolean）：是否创建成功
   * @throws gRPC `InvalidArgument` — path 为空
   */
  createDirectory(request: CreateDirectoryRequest): Promise<CreateDirectoryResponse> {
    return promisifyUnary<CreateDirectoryRequest, CreateDirectoryResponse>(this.client, 'createDirectory', request);
  }

  /**
   * 删除目录（递归删除所有子目录和文件）。
   *
   * @param request 请求参数：
   *   - `path`（string，必填）：要删除的目录路径
   * @returns 包含 `success`（boolean）：是否删除成功
   * @throws gRPC `InvalidArgument` — path 为空
   */
  deleteDirectory(request: DeleteDirectoryRequest): Promise<DeleteDirectoryResponse> {
    return promisifyUnary<DeleteDirectoryRequest, DeleteDirectoryResponse>(this.client, 'deleteDirectory', request);
  }

  /**
   * 列出目录下的文件和子目录名称。
   *
   * @param request 请求参数：
   *   - `path`（string，必填）：目录路径
   *   - `pattern`（string，可选）：搜索模式（如 `*.txt`），为空则返回所有
   * @returns 包含 `names`（string[]）：文件和目录名称列表
   * @throws gRPC `InvalidArgument` — path 为空
   */
  listDirectory(request: ListDirectoryRequest): Promise<ListDirectoryResponse> {
    return promisifyUnary<ListDirectoryRequest, ListDirectoryResponse>(this.client, 'listDirectory', request);
  }

  /**
   * 移动目录到目标路径。
   *
   * @param request 请求参数：
   *   - `sourcePath`（string，必填）：源目录路径
   *   - `targetPath`（string，必填）：目标目录路径
   *   - `overwrite`（boolean）：是否覆盖已存在的目标目录
   * @returns 包含 `success`（boolean）：是否移动成功
   * @throws gRPC `InvalidArgument` — sourcePath 或 targetPath 为空
   */
  moveDirectory(request: MoveDirectoryRequest): Promise<MoveDirectoryResponse> {
    return promisifyUnary<MoveDirectoryRequest, MoveDirectoryResponse>(this.client, 'moveDirectory', request);
  }

  /**
   * 在指定根目录下递归搜索匹配模式的文件。
   *
   * @param request 请求参数：
   *   - `rootPath`（string，必填）：搜索根目录
   *   - `pattern`（string，可选）：搜索模式（如 `*.txt`），为空则匹配所有文件
   * @returns 包含 `paths`（string[]）：匹配文件的完整路径列表
   * @throws gRPC `InvalidArgument` — rootPath 为空
   */
  searchFiles(request: SearchFilesRequest): Promise<SearchFilesResponse> {
    return promisifyUnary<SearchFilesRequest, SearchFilesResponse>(this.client, 'searchFiles', request);
  }

  /**
   * 在系统常见位置（桌面、开始菜单等）查找指定可执行文件的快捷方式。
   *
   * @param request 请求参数：
   *   - `exeName`（string，必填）：可执行文件名（如 `chrome.exe`）
   * @returns 包含 `targetPaths`（string[]）：找到的快捷方式目标路径列表
   * @throws gRPC `InvalidArgument` — exeName 为空
   */
  findExeShortcuts(request: FindExeShortcutsRequest): Promise<FindExeShortcutsResponse> {
    return promisifyUnary<FindExeShortcutsRequest, FindExeShortcutsResponse>(
      this.client,
      'findExeShortcuts',
      request
    );
  }
}
