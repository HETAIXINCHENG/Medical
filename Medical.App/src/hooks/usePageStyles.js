import { useEffect } from 'react';

export default function usePageStyles(fileName) {
  useEffect(() => {
    if (!fileName) return undefined;
    const link = document.createElement('link');
    link.rel = 'stylesheet';
    link.href = `/styles/${fileName}`;
    document.head.appendChild(link);

    return () => {
      document.head.removeChild(link);
    };
  }, [fileName]);
}
