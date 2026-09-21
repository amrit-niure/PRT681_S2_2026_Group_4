"use client";

import { useSyncExternalStore } from "react";

/**
 * Subscribes to a CSS media query. The server render (and first hydration pass) reports `false`,
 * then React re-renders with the real value, so markup never mismatches during hydration.
 */
export function useMediaQuery(query: string): boolean {
  return useSyncExternalStore(
    (onChange) => {
      const list = window.matchMedia(query);
      list.addEventListener("change", onChange);
      return () => list.removeEventListener("change", onChange);
    },
    () => window.matchMedia(query).matches,
    () => false,
  );
}
