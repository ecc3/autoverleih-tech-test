import { useTranslation } from "react-i18next";

const localeMap: Record<string, string> = {
  en: "en-GB",
  de: "de-DE",
};

export function useLocale(): string {
  const { i18n } = useTranslation();
  return localeMap[i18n.language] ?? "en-GB";
}
