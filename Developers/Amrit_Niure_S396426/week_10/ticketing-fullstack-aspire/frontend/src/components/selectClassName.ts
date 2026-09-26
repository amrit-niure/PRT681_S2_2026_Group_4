import { cn } from 'cn'

/** Matches the shadcn <Input> look for native <select> elements. */
export const selectClassName = cn(
  'h-8 min-w-0 rounded-lg border border-input bg-transparent px-2.5 py-1 text-base transition-colors outline-none focus-visible:border-ring focus-visible:ring-3 focus-visible:ring-ring/50 disabled:pointer-events-none disabled:opacity-50 md:text-sm dark:bg-input/30',
)
