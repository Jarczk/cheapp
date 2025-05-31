'use client';

import { useSearchParams } from 'next/navigation';
import { useEffect, useState } from 'react';

interface Offer {
  id: string;
  title: string;
  price: number;
  currency: string;
  marketplace: string;
  url: string;
}

export default function SearchPage() {
  const searchParams = useSearchParams();
  const query = searchParams.get('q');
  const [offers, setOffers] = useState<Offer[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!query) return;

    const fetchOffers = async () => {
      try {
        setLoading(true);
        const response = await fetch(`http://localhost:5166/api/offers?q=${encodeURIComponent(query)}`);
        if (!response.ok) throw new Error('Failed to fetch offers');
        const data = await response.json();
        setOffers(data);
      } catch (err) {
        setError(err instanceof Error ? err.message : 'An error occurred');
      } finally {
        setLoading(false);
      }
    };

    fetchOffers();
  }, [query]);

  if (!query) return <div className="text-center py-10">No search query provided</div>;
  if (loading) return <div className="text-center py-10">Loading...</div>;
  if (error) return <div className="text-center py-10 text-red-600">{error}</div>;

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <h1 className="text-2xl font-bold mb-6">Search Results for "{query}"</h1>
      
      <div className="grid gap-6">
        {offers.map((offer) => (
          <div key={offer.id} className="bg-white rounded-lg shadow p-6">
            <h2 className="text-lg font-semibold mb-2">{offer.title}</h2>
            <div className="flex justify-between items-center">
              <div>
                <p className="text-2xl font-bold text-blue-600">
                  {offer.price} {offer.currency}
                </p>
                <p className="text-sm text-gray-500">{offer.marketplace}</p>
              </div>
              <a
                href={offer.url}
                target="_blank"
                rel="noopener noreferrer"
                className="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700 transition"
              >
                View Deal
              </a>
            </div>
          </div>
        ))}

        {offers.length === 0 && (
          <div className="text-center py-10 text-gray-500">
            No offers found for "{query}"
          </div>
        )}
      </div>
    </div>
  );
}