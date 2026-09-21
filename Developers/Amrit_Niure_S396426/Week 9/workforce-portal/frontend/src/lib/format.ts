// Fixed locale so the server render and the browser produce identical text (no hydration mismatch).
const LOCALE = "en-AU";

const pad = (value: number) => String(value).padStart(2, "0");

/** Date -> "yyyy-MM-dd" using the local calendar day (never toISOString, which shifts by time zone). */
export const toDateOnly = (date: Date) => `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`;

/** "yyyy-MM-dd" -> Date at local midnight. */
export const fromDateOnly = (value: string) => {
  const [year, month, day] = value.split("-").map(Number);
  return new Date(year, month - 1, day);
};

export const formatDate = (dateOnly: string) =>
  fromDateOnly(dateOnly).toLocaleDateString(LOCALE, { day: "2-digit", month: "short", year: "numeric" });

const currencyFormat = new Intl.NumberFormat(LOCALE, {
  style: "currency",
  currency: "AUD",
  maximumFractionDigits: 0,
});

export const formatCurrency = (amount: number) => currencyFormat.format(amount);
