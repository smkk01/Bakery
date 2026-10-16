import { useEffect, useMemo, useState, type ReactNode } from 'react'

type SortValue = string | number

export interface AdminTableColumn<T> {
  key: string
  label: string
  searchValue: (row: T) => string
  sortValue?: (row: T) => SortValue
  render: (row: T) => ReactNode
}

interface AdminTableProps<T> {
  rows: T[]
  columns: AdminTableColumn<T>[]
  rowKey: (row: T) => string
  emptyMessage: string
  searchPlaceholder: string
}

const pageSize = 8

export function AdminTable<T>({
  rows,
  columns,
  rowKey,
  emptyMessage,
  searchPlaceholder,
}: AdminTableProps<T>) {
  const [search, setSearch] = useState('')
  const [sortKey, setSortKey] = useState(columns[0]?.key ?? '')
  const [sortDirection, setSortDirection] = useState<'asc' | 'desc'>('asc')
  const [page, setPage] = useState(1)

  const filteredRows = useMemo(() => {
    const query = search.trim().toLocaleLowerCase()
    const filtered = rows.filter((row) =>
      columns.some((column) => column.searchValue(row).toLocaleLowerCase().includes(query)),
    )
    const sortColumn = columns.find((column) => column.key === sortKey)
    if (!sortColumn?.sortValue) {
      return filtered
    }

    return [...filtered].sort((first, second) => {
      const firstValue = sortColumn.sortValue?.(first) ?? ''
      const secondValue = sortColumn.sortValue?.(second) ?? ''
      const comparison = typeof firstValue === 'number' && typeof secondValue === 'number'
        ? firstValue - secondValue
        : String(firstValue).localeCompare(String(secondValue), undefined, { numeric: true, sensitivity: 'base' })
      return sortDirection === 'asc' ? comparison : -comparison
    })
  }, [columns, rows, search, sortDirection, sortKey])

  const pageCount = Math.max(1, Math.ceil(filteredRows.length / pageSize))
  const currentPage = Math.min(page, pageCount)
  const visibleRows = filteredRows.slice((currentPage - 1) * pageSize, currentPage * pageSize)

  useEffect(() => {
    if (page !== currentPage) setPage(currentPage)
  }, [currentPage, page])

  function changeSearch(value: string) {
    setSearch(value)
    setPage(1)
  }

  function toggleSort(column: AdminTableColumn<T>) {
    if (!column.sortValue) return
    if (sortKey === column.key) {
      setSortDirection((direction) => direction === 'asc' ? 'desc' : 'asc')
    } else {
      setSortKey(column.key)
      setSortDirection('asc')
    }
    setPage(1)
  }

  return (
    <div>
      <div className="d-flex flex-column flex-sm-row align-items-sm-center justify-content-between gap-3 mb-3">
        <label className="visually-hidden" htmlFor={`admin-search-${columns[0]?.key ?? 'table'}`}>
          Search table
        </label>
        <input
          className="form-control admin-table-search"
          id={`admin-search-${columns[0]?.key ?? 'table'}`}
          onChange={(event) => changeSearch(event.target.value)}
          placeholder={searchPlaceholder}
          type="search"
          value={search}
        />
        <span className="small text-secondary" aria-live="polite">
          {filteredRows.length} result{filteredRows.length === 1 ? '' : 's'}
        </span>
      </div>

      <div className="table-responsive rounded-3 border">
        <table className="table table-hover align-middle mb-0">
          <thead className="table-light">
            <tr>
              {columns.map((column) => (
                <th key={column.key} scope="col">
                  {column.sortValue ? (
                    <button
                      aria-label={`Sort by ${column.label}`}
                      className="btn btn-link text-dark text-decoration-none fw-semibold p-0"
                      onClick={() => toggleSort(column)}
                      type="button"
                    >
                      {column.label}
                      {sortKey === column.key && (
                        <span aria-hidden="true" className="ms-1">
                          {sortDirection === 'asc' ? '↑' : '↓'}
                        </span>
                      )}
                    </button>
                  ) : column.label}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {visibleRows.map((row) => (
              <tr key={rowKey(row)}>
                {columns.map((column) => <td key={column.key}>{column.render(row)}</td>)}
              </tr>
            ))}
            {visibleRows.length === 0 && (
              <tr>
                <td className="py-5 text-center text-secondary" colSpan={columns.length}>
                  {search ? 'No results match your search.' : emptyMessage}
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

      {filteredRows.length > pageSize && (
        <nav aria-label="Table pages" className="d-flex justify-content-between align-items-center mt-3">
          <span className="small text-secondary">Page {currentPage} of {pageCount}</span>
          <div className="btn-group">
            <button
              className="btn btn-outline-secondary btn-sm"
              disabled={currentPage <= 1}
              onClick={() => setPage((value) => Math.max(1, value - 1))}
              type="button"
            >
              Previous
            </button>
            <button
              className="btn btn-outline-secondary btn-sm"
              disabled={currentPage >= pageCount}
              onClick={() => setPage((value) => Math.min(pageCount, value + 1))}
              type="button"
            >
              Next
            </button>
          </div>
        </nav>
      )}
    </div>
  )
}
