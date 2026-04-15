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
 * `FsService` 客户端（包 `swg.fs`）。
 */
export class FsServiceClient {
  constructor(private readonly client: grpc.Client) {}

  /** 读取文本文件。 */
  readText(request: ReadTextRequest): Promise<ReadTextResponse> {
    return promisifyUnary<ReadTextRequest, ReadTextResponse>(this.client, 'readText', request);
  }
  /** 覆盖写入文本文件。 */
  writeText(request: WriteTextRequest): Promise<WriteTextResponse> {
    return promisifyUnary<WriteTextRequest, WriteTextResponse>(this.client, 'writeText', request);
  }
  /** 追加写入文本文件。 */
  appendText(request: AppendTextRequest): Promise<AppendTextResponse> {
    return promisifyUnary<AppendTextRequest, AppendTextResponse>(this.client, 'appendText', request);
  }
  /** 复制文件。 */
  copyFile(request: CopyFileRequest): Promise<CopyFileResponse> {
    return promisifyUnary<CopyFileRequest, CopyFileResponse>(this.client, 'copyFile', request);
  }
  /** 移动文件。 */
  moveFile(request: MoveFileRequest): Promise<MoveFileResponse> {
    return promisifyUnary<MoveFileRequest, MoveFileResponse>(this.client, 'moveFile', request);
  }
  /** 删除文件。 */
  deleteFile(request: DeleteFileRequest): Promise<DeleteFileResponse> {
    return promisifyUnary<DeleteFileRequest, DeleteFileResponse>(this.client, 'deleteFile', request);
  }
  /** 检查路径是否存在。 */
  exists(request: ExistsRequest): Promise<ExistsResponse> {
    return promisifyUnary<ExistsRequest, ExistsResponse>(this.client, 'exists', request);
  }
  /** 获取文件或目录元信息。 */
  getItemInfo(request: GetItemInfoRequest): Promise<GetItemInfoResponse> {
    return promisifyUnary<GetItemInfoRequest, GetItemInfoResponse>(this.client, 'getItemInfo', request);
  }
  /** 创建目录。 */
  createDirectory(request: CreateDirectoryRequest): Promise<CreateDirectoryResponse> {
    return promisifyUnary<CreateDirectoryRequest, CreateDirectoryResponse>(this.client, 'createDirectory', request);
  }
  /** 删除目录。 */
  deleteDirectory(request: DeleteDirectoryRequest): Promise<DeleteDirectoryResponse> {
    return promisifyUnary<DeleteDirectoryRequest, DeleteDirectoryResponse>(this.client, 'deleteDirectory', request);
  }
  /** 列举目录项。 */
  listDirectory(request: ListDirectoryRequest): Promise<ListDirectoryResponse> {
    return promisifyUnary<ListDirectoryRequest, ListDirectoryResponse>(this.client, 'listDirectory', request);
  }
  /** 移动目录。 */
  moveDirectory(request: MoveDirectoryRequest): Promise<MoveDirectoryResponse> {
    return promisifyUnary<MoveDirectoryRequest, MoveDirectoryResponse>(this.client, 'moveDirectory', request);
  }
  /** 按 pattern 搜索文件。 */
  searchFiles(request: SearchFilesRequest): Promise<SearchFilesResponse> {
    return promisifyUnary<SearchFilesRequest, SearchFilesResponse>(this.client, 'searchFiles', request);
  }
  /** 查找可执行文件快捷方式。 */
  findExeShortcuts(request: FindExeShortcutsRequest): Promise<FindExeShortcutsResponse> {
    return promisifyUnary<FindExeShortcutsRequest, FindExeShortcutsResponse>(
      this.client,
      'findExeShortcuts',
      request
    );
  }
}
